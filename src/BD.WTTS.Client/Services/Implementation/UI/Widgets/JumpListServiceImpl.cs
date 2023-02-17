#if WINDOWS
using Avalonia.Win32.JumpLists;
using static BD.WTTS.Services.IJumpListService;
using AvaloniaApplication = Avalonia.Application;
using WinUI = Windows.UI.StartScreen;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IJumpListService"/>
internal sealed class JumpListServiceImpl : IJumpListService
{
#if WINDOWS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsWindows10JumpListSupported()
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10586))
        {
            return WinUI.JumpList.IsSupported();
        }
        return false;
    }
#endif

    ValueTask IJumpListService.AddJumpItemsAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    {
#if WINDOWS
        return AddJumpItemsByWinAsync(items);
#else
        return default;
#endif
    }

#if WINDOWS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static async ValueTask AddJumpItemsByWinAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    {
        if (IsWindows10JumpListSupported() && await AddJumpItemsByWin10Async(items)) return;
        await AddJumpItemsByWPFAsync(items);
    }

    static async ValueTask<bool> AddJumpItemsByWin10Async(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    {
        try
        {
            var jumpList1 = await WinUI.JumpList.LoadCurrentAsync();

            foreach (var (title, _, _, arguments, description, customCategory) in items)
            {
                var isAdd = false;
                var item = jumpList1.Items.FirstOrDefault(x =>
                    x.Kind == WinUI.JumpListItemKind.Arguments && x.Arguments == arguments);
                if (item == null)
                {
                    item = WinUI.JumpListItem.CreateWithArguments(arguments, title);
                    isAdd = true;
                }
                else
                {
                    item.DisplayName = title;
                }
                item.Description = description;
                item.GroupName = customCategory;

                if (isAdd) jumpList1.Items.Add(item);
            }

            await jumpList1.SaveAsync();
            return true;
        }
        catch (Exception ex)
        {
            const string log_msg = $"{nameof(AddJumpItemsByWin10Async)} catch.";
            Log.Error(TAG, ex, log_msg);
            return false;
        }
    }

    static ValueTask<bool> AddJumpItemsByWPFAsync(IEnumerable<(string title, string applicationPath, string iconResourcePath, string arguments, string description, string customCategory)> items)
    {
        try
        {
            var avaloniaApp = AvaloniaApplication.Current;
            avaloniaApp.ThrowIsNull();

            var jumpList1 = JumpList.GetJumpList(avaloniaApp);
            if (jumpList1 == null)
            {
                jumpList1 = new JumpList
                {
                    ShowRecentCategory = true,
                    ShowFrequentCategory = false,
                };
                JumpList.SetJumpList(avaloniaApp, jumpList1);
            }

            foreach (var (title, applicationPath, iconResourcePath, arguments, description, customCategory) in items)
            {
                var task = (from s in jumpList1.JumpItems
                            let v = s is JumpTask t ? t : null
                            where v != null &&
                              v.ApplicationPath == applicationPath &&
                              v.Arguments == arguments
                            select v).FirstOrDefault();
                if (task != null)
                {
                    task.Title = title;
                    task.IconResourcePath = iconResourcePath;
                    task.Description = description;
                    task.CustomCategory = customCategory;
                }
                else
                {
                    task = new JumpTask
                    {
                        // Get the path to Calculator and set the JumpTask properties.
                        ApplicationPath = applicationPath,
                        IconResourcePath = iconResourcePath,
                        Arguments = arguments,
                        Title = title,
                        Description = description,
                        CustomCategory = customCategory,
                    };
                    jumpList1.JumpItems.Add(task);
                }
            }

            jumpList1.Apply();
            return new(true);
        }
        catch (Exception ex)
        {
            const string log_msg = $"{nameof(AddJumpItemsByWPFAsync)} catch.";
            Log.Error(TAG, ex, log_msg);
            return new(false);
        }
    }
#endif
}
#endif