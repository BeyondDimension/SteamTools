---
name: "(English)Bug Report \U0001F41B"
about: Create a report to help us improve
title: "\U0001F41B[BUG] Please enter a title"
labels: bug
assignees: ''

---

## Please check each item in the following list carefully
- [ ] I have searched and found no similar issues
- [ ] Have searched the documentation and found no relevant content
- [ ] I have tried to use the latest version, but the problem still exists
- [ ] The official version is used (no replacement and modification of the installation directory program files)

<!--
Before posting, try searching Issues to see if your issue has already been raised.
Describe the bug in detail so that everyone can understand it, please be sure that what you are posting is a bug, if you are not sure, please post a Question
Don't forget to fill in the title, which should be a short description of the problem
-->

### 🐛 Description
<!--
Describe the problem in detail
-->


### 📷 Steps to Reproduce

<!--
Clearly describe the reproduction steps so that others can see the problem
Ensure that the above steps are 100% reproducible on most devices, if possible, to pinpoint the cause of the problem
-->
1. 
2. 
3. 

### 📄 Log Information
<!--

```
- Path to the folder where the log files are located
    - Microsoft Store ```%USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalCache\Logs```
    - Windows ```\Logs``` or ```%Tmp%\Steam++\Logs```
    - macOS ```~/Library/Caches/Steam++/Logs```
    - Linux ```$XDG_CACHE_HOME/Steam++/Logs``` or ```$HOME/.cache/Steam++/Logs```
    - Android ```/data/data/net.steampp.app/cache/Logs```
Your log content
```

-->

### 🖼 Screenshots

<!--
Screenshots can be posted here
-->


### 🚑 Other Information
- Processor architecture: <!-- x64 / x86 / arm64 -->
- System version number: <!-- example Windows 10.19042.844(see winver) / macOS Monterey 12 / Ubuntu 20.04.2 LTS -->
- Download channels: <!-- Microsoft Store / Official Website / Github / AppStore / xx netdisk -->
- Program version:
    <!-- 
    - In the app: ⚙ → **About** (applies to all download channels)
    - Or run the following command:
        ```powershell
        $exe = @(
            (Get-Process 'Steam++' -ErrorAction SilentlyContinue).Path
            $storeLocation = (Get-AppxPackage -Name '4651ED44255E.47979655102CE').InstallLocation
            if ($storeLocation) { Join-Path $storeLocation 'Steam++.exe' }
            (Get-Command 'Steam++' -ErrorAction SilentlyContinue).Source
            'HKLM:\SOFTWARE', 'HKLM:\SOFTWARE\WOW6432Node', 'HKCU:\SOFTWARE' | ForEach-Object { Join-Path $_ 'Microsoft\Windows\CurrentVersion\Uninstall\*' } | Get-ItemProperty -ErrorAction SilentlyContinue | Where-Object { $_.DisplayName -match 'Watt Toolkit|Steam\+\+' } | ForEach-Object { if ($_.InstallLocation) { Join-Path $_.InstallLocation 'Steam++.exe' } elseif ($_.DisplayIcon) { $_.DisplayIcon -replace ',\d+$' } }
        ) | Where-Object { $_ -and (Test-Path $_) } | Select-Object -First 1
        if ($exe) { ($exe | Get-Item).VersionInfo.ProductVersion } else { 'Steam++.exe not found automatically, check **Settings** → **About** for its version' }
        ```
    -->
- Does it contain a standalone runtime: <!-- with_runtime or without fde in the file name -->