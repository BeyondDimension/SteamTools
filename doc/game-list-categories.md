# Library categories

Open **Library Games → Categories** to create a manual, automatic, or mixed category. The rule builder previews edits against the available library and shows the conditions that matched each game. A template opens as a draft. **Save category** persists the definition. **Run all auto categories** evaluates saved automatic and mixed definitions and stores their membership. The overlap tab lists games that belong to more than one applied category.

Manual mode uses included App IDs. Mixed mode combines those IDs with automatic matches. In both modes, an exclusion takes precedence. Automatic mode uses the rule tree. Missing values, invalid operands and unknown fields produce an unknown condition, including inside NOT. A rule tree must evaluate to true to match.

## Data and persistence

| Source | Fields and limits |
| --- | --- |
| Existing Steam app service and models | The live owned-app feed, installed state, disk use from app manifests, app type, OS, release year, developer and publisher. With Steam closed, the page uses locally evidenced App IDs from manifests, play history and Steam favorites or hidden collections, plus manual App IDs for each category. The offline library can omit unplayed, uninstalled games. |
| Most recent local account's `localconfig.vdf` | Total and recent playtime when present, plus last-played date. Date comparisons use UTC. |
| Local Steam collection and Toolkit settings | Steam Favorites and Hidden membership from namespace 1. The hidden rule also includes games hidden in Watt Toolkit. |
| Local Steam achievement caches | The current account's `UserGameStats_<accountid>_<appid>.bin` and the corresponding schema. The page reuses the existing achievement schema reader. Completion covers base-game achievements, including archived achievements already unlocked. The count uses known schema definitions or the Steam store total. |
| Steam Store `appdetails` | Regional current and original price, discount, free-to-play, release year, genres, publisher, developer, Metacritic, controller, language, OS and single/multiplayer store flags. |
| Steam Store product page and `IStoreBrowseService/GetItems` | The product's publicly displayed English community tags and batched Steam Deck compatibility, respectively. The tag picker suggests values present in the local cache. |
| Steamworks `IUserReviewsService/GetAppReviews` | Aggregated all-language review totals and Steam's review tier. Positive review percentage is positive count divided by total reviews. [Valve review API documentation](https://partner.steamgames.com/doc/webapi/IUserReviewsService) |

Purchase date and family ownership currently remain unknown. The available local files and service interfaces in this checkout do not provide reliable account-wide facts for them. The **Unplayed backlog** draft includes the requested acquisition-date rule and returns matches only when both operands are available.

**Value rating** is positive review percentage divided by `1 + price per hour`. Price per hour is current regional store price divided by `max(total played hours, 0.1)`. The comparison uses the store's currency units. For example, 95% positive, 2 hours played and a regional price of 10 yields a price per hour of 5 and a value rating of about 15.83.

Local definitions, applied membership and the selected region are stored below the application data directory in `Plugins/GameList/categories.json`. The import and export document carries only versioned definitions and manual overrides. Import replaces matching category IDs after confirmation. The app writes its files via a sibling temporary file and atomic replacement and uses source-generated JSON metadata.

Public store requests use Toolkit's general HTTP client with cookies disabled. Only public App IDs and the selected country code are needed. **Refresh store data** processes batches of 30 App IDs, separates requests by at least two seconds, and honors the server's rate-limit retry time. The cache is under `Plugins/GameList/categories-store-v1/<region>/<appid>.json` in the application cache directory. Price expiry is 24 hours. The other expiry intervals are seven days. Completed sources checkpoint independently. Cached values remain usable if a later refresh is interrupted or unavailable. The initial store region is CN, configurable on the page, and the currency shown comes from the actual Steam price responses.

## Steam collection export

The installed Windows client reported executable version `11.27.33.03`. Read-only inspection confirmed each of these files below `userdata/<accountid>/config/cloudstorage/`.

| File | Observed role and format |
| --- | --- |
| `cloud-storage-namespace-1.json` | An array of `[key, entry]` pairs. Collection keys start with `user-collections.`. Each entry has a Unix-seconds timestamp and a string version. Its `value` is embedded JSON with ID, name and added/removed App IDs. Dynamic collections include a filter specification. Deleted entries use tombstones. |
| `cloud-storage-namespace-1.modified.json` | Companion file for pending local changes. No pending collection entry was present in the inspected samples. |
| `cloud-storage-namespaces.json` | Pairs of integer namespace IDs and string revision counters. Namespace IDs 1, 3 and 4 were present. |

The older local config layout and current cloud layout are described in implementations that read Steam collection files. Current third-party writers also differ in their sync metadata handling. Read-only samples establish the on-disk representation and cannot verify Valve's reconciliation behavior for newly written records, conflict-resolution markers, or interrupted updates across the three files. [Steam ROM Manager collection backend](https://github.com/SteamGridDB/steam-rom-manager/blob/master/src/lib/category-manager.ts), [Steam Library Organizer storage analysis](https://github.com/okabeeer/SteamLibraryOrganizer#where-steam-keeps-collections), [Valve Steam Cloud documentation](https://help.steampowered.com/en/faqs/view/68D2-35AB-09A9-7678)

`ISteamCollectionWriter` implements the export-only fallback. **Preview Steam collection export** shows the computed name and App IDs in a versioned Watt Toolkit document with clipboard and file options. Steam's own sync files, client process and app manifests are outside this export path. No live Steam write or cloud acceptance test was performed.

## Change inventory

- `src/BD.WTTS.Client.Plugins.GameList/Categories/` contains the model, field registry, rule evaluator, app-data JSON store, read-only Steam collection export, inventory, local achievements and public metadata cache.
- `src/BD.WTTS.Client.Plugins.GameList/UI/` contains the Categories view, rule builder, view model and embedded tab entry. `Plugins/Plugin.cs` registers services.
- `src/BD.WTTS.Client/Resources/Strings{,.en}.resx` and `Strings.Designer.cs` provide the Simplified Chinese and English strings.
- `src/BD.WTTS.UnitTest/` contains the rule, store parser and local achievements tests.

## Verification

The Windows plugin and Avalonia application built for `net11.0-windows10.0.19041.0`. The plugin also built for `net11.0` using the local NuGet fallback package cache. The existing dependency and application warnings remain. The macOS target stopped at `NETSDK1147` because this SDK installation lacks the `macos` workload. The NUnit adapter ran 23 category tests covering rule evaluation, missing data, price and review values, tags, Steam Deck and synthetic local achievement data. All 23 passed.

```powershell
$win = 'net11.0-windows10.0.19041.0'
$plugin = 'src/BD.WTTS.Client.Plugins.GameList/BD.WTTS.Client.Plugins.GameList.csproj'
$app = 'src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj'
$test = 'src/BD.WTTS.UnitTest/BD.WTTS.UnitTest.csproj'
$buildOptions = @('-p:NuGetAudit=false', '-p:UsedAvaloniaProducts=', '-m:4')
dotnet build $plugin -c Debug -f $win "-p:WttsDesktopTargetFrameworks=$win" @buildOptions
dotnet build $app -c Debug -f $win "-p:WttsDesktopTargetFrameworks=$win" @buildOptions
dotnet build $plugin -c Debug -f net11.0 '-p:WttsDesktopTargetFrameworks=net11.0' '-p:RestoreFallbackFolders=C:/Users/Dark/.nuget/packages' --ignore-failed-sources @buildOptions
dotnet build $plugin -c Debug -f net11.0-macos '-p:WttsDesktopTargetFrameworks=net11.0-macos' @buildOptions
dotnet build $test -c Debug "-p:WttsDesktopTargetFrameworks=$win" @buildOptions
dotnet test "src/BD.WTTS.UnitTest/bin/Debug/$win/BD.WTTS.UnitTest.dll" --filter 'FullyQualifiedName~GameCategoryRuleTest|FullyQualifiedName~CategoryStoreParserTest|FullyQualifiedName~CategoryLocalAchievementsTest'
```

Live store requests from the validation shell timed out. Public payloads were covered by parser fixtures. The desktop UI checklist remains for a machine with live Steam Store connectivity.

## Manual checklist

1. With Steam closed, open **Library Games → Categories**. Check the empty state and add a manual category by searching installed games or entering an App ID. Save, leave the tab and reopen it.
2. Add a template and verify it appears as a draft. Combine **All**, **Any** and **Not** with a numeric comparison, a range, and a tag condition. Check that covers, counts and matching conditions update while editing.
3. In mixed mode, include a game outside the automatic rule and exclude an automatic match. Run saved categories and check the applied counts and game names in the overlap tab.
4. Refresh metadata in the desired region. Check the displayed currency, cancel midway, then resume. Disconnect the network and confirm that saved rules and cached data still produce a preview.
5. Export definitions, reopen the tab and re-import. Check matching IDs are replaced after confirmation. Verify Steam collection preview copies and exports the same App IDs.
