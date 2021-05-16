# [Windows Authenticator](https://github.com/winauth/winauth)

*WinAuth is a portable, open-source Authenticator for Windows that provides counter or time-based RFC 6238 authenticators and common implementations, such as the Google Authenticator. WinAuth can be used with many Bitcoin trading websites as well as games, supporting Battle.net (World of Warcraft, Hearthstone, Heroes of the Storm, Diablo), Guild Wars 2, Glyph (Rift and ArcheAge), WildStar, RuneScape, SWTOR and Steam.*

----

## Download Latest Beta Version

The latest beta version is WinAuth 3.6 and is available from the [releases](https://github.com/winauth/winauth/releases) page. Please do not use this version unless you are testing new features.

This will likely be the last version of WinAuth when released as 3.7. It has reached the end of its useful life and requires a massive UI overhaul. My thanks to everyone who has used WinAuth, provided fixes and useful suggestions.

----

## Download Latest Stable Version (WinAuth 3.5)

WinAuth provides an alternative solution to combine various two-factor authenticator services in one convenient place.

This is the latest stable version and can be downloaded from the [releases](https://github.com/winauth/winauth/releases) page, or directly from:

[Latest Version (WinAuth-3.5.1)](https://github.com/winauth/winauth/releases/download/3.5.1/WinAuth-3.5.1.zip)

There is also a [.Net 3.5 build of WinAuth](https://github.com/winauth/winauth/releases/download/3.5.1/WinAuth-3.5.1-NET35.zip) that can be run on Windows 7 installations "out of the box".

<img src="https://winauth.github.io/winauth/images/winauth3-preview.png" alt="WinAuth3 Preview" />

Features include:

  * Support for time-based RFC 6238 authenticators (e.g. Google Authenticator) and HOTP counter-based authenticators
  * Supports Battle.net (World of Warcraft, Hearthstone, Heroes of the Storm, Diablo III), GuildWars 2, Trion / Glyph (Rift, ArcheAge), RuneScape, WildStar, SWTOR and Steam
  * Supports Steam's SteamGuard and trading confirmations
  * Supports many Bitcoin trading websites such as Coinbase, Gemini, Circle, Bitstamp, BTC-e, Cryptsy
  * Displays multiple authenticators simultaneously
  * Codes displayed and refreshed automatically or on demand
  * Data is protected with your password, locked to Windows machine or account, or a YubiKey
  * Additional password protection per authenticator
  * Restore features for supported authenticators, e.g. Battle.net and Rift
  * Selection of standard or custom icons
  * Hot-key binding with standard or custom actions, such as code notification, keyboard input, and copy to clipboard
  * Portable mode preventing changes to other files or registry settings
  * Import and export in UriKeyFormat and from Authenticator Plus for Android 

#### Updates

  * 3.5.1 - Issue#366: fix spamming notifications when Steam is down. Hide confim/cancel all buttons on login.

### How To Use

Use the following link to download the latest version of WinAuth, or go to the [Releases](https://github.com/winauth/winauth/releases) page.

Requires [Microsoft .Net 4.5](http://www.microsoft.com/en-us/download/details.aspx?id=30653)

To use:
  * Download the latest stable version of WinAuth
  * There is no installation required, just open the zip file and extract the WinAuth.exe file to anywhere on your computer
  * Double-click or run WinAuth.exe
  * Click the Add button to add or import an authenticator
  * Right-click any authenticator to bring up context menu
  * Click the icon on the right to show the current code, if auto-refresh is not enabled
  * Click cog/options icon for program options

To compile and build from source:
  * Download source code file or clone project
  * Requires Microsoft Visual Studio 2015
  * any other dependencies are included in the source tree in the 3rd Party folder
  * Use [ILMerge](http://research.microsoft.com/en-us/people/mbarnett/ilmerge.aspx ) to combine assemblies into one single exe file

### New Features

Version 3.5 includes Steam trade confirmations.

<img src="https://winauth.github.io/winauth/images/2013/07/steamconf1.png" alt="steamconf" class="aligncenter" />

If you registered SteamGuard with WinAuth 3.3.7 or earlier, you will need to remove it from your Steam account and add it again. This is because WinAuth 3.3 only kept information relevant to generating the SteamGuard authenticator codes, however, confirmations needs more information.

You can go into the normal Steam client, choose Account Details, then click "Manage Steam Guard". Click the Remove Authenticator button and enter the recovery code (aka revocation code - found from right-clicking in WinAuth).

Once the new authenticator is added, you will have an extra option when right-clicking called "Confirmations...". This will login with your username/password and show your current trade confirmations. You can click to view more details, and use the buttons to accept or reject them.

If you choose "remember me", WinAuth will keep you logged in (does not keep your username/password) so you can quickly go into Confirmations again.

----

## COMMON QUESTIONS

#### Is it secure? Is it safe?

All authenticators just provide another layer of security. None are 100% effective.

A physical/keychain device is by far the best protection. Although still subject to any man-in-the-middle attack, there is no way to get at the secret key stored within it. If you are at all concerned, get one of these.

An iPhone app or app on a non-rooted Android device is also secure. There is no way to get at the secret key stored on the device, however, some apps provides way to export the key that could compromise your authenticator if you do not physically protect your phone. Also if those apps backup their data elsewhere, that data could be vulnerable.

A rooted-Android phone can have your secret key read off it by an app with access. Some apps also do not encrypt the keys and so this should be considered risky.

WinAuth stores you secret key in an encrypted file on your computer. Whilst it cannot therefore provide the same security as a separate physical device, as much as possible has been done to protect the key on your machine. As above, physical access to your machine would be the only way to compromise any authenticator.

#### I'm concerned this might be a virus / malware / keylogger

WinAuth has been around and used since mid-2010 and has been downloaded by thousands of users.

It has always been open-source allowing everyone to inspect and review the code. A binary is provided, but the source code is always released simultaneously so that you can review the code and build it yourself.

No personal information is sent out to any other 3rd party servers. It never even sees your account information, only your authenticator details.

There are no other executables installed on your machine. There is no installer doing things you are unable to monitor. WinAuth is portable so you can just run it from anywhere.

#### I found WinAuth on another website, is it the same thing?

WinAuth source code is uploaded to GitHub at http://github.com/winauth/winauth and pre-built binaries are in [releases](https://github.com/winauth/winauth/releases). It is not published anywhere else, so please do not download any other programs claiming to be WinAuth.

#### Where does WinAuth save my authenticator information?

Unlike some other authenticator applications, WinAuth does not store/send your information to any 3rd party servers. Your authenticator information is saved by default in your account roaming profile, i.e. c:\Users\<username>\AppData\Roaming\WinAuth. However, this file can be moved anywhere and passed into WinAuth when run.

----

## More Information

All trademarks are recognised, including but not limited to:

  * Blizzard, Battle.net, World of Warcraft, Starcraft, Diablo
  * ArenaNet, Guild Wars 2
  * Trion, Rift
  * Google
  * Microsoft
  * Steam

----

## Author

WinAuth was written by Colin Mackie. Copyright (C) 2010-2017.

Bitcoin donations can be sent to `1C4bMkMATViiWYsmJSDUx2MruWM785C36Y`

----

## License

This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License  along with this program.  If not, see http://www.gnu.org/licenses/.
