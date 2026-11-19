# Steam++ v3.X FileSystem Document

### 📁 Storage
- AppData
    - Microsoft Store ```%USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalState```
    - Windows ```\AppData``` or ```%LocalAppData%\Steam++```
    - macOS ```~/Library/Steam++```
    - Linux ```$XDG_DATA_HOME/Steam++``` or ```$HOME/.local/share/Steam++```
    - Android ```/data/data/net.steampp.app/files```
- Cache
    - Microsoft Store ```%USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalCache```
    - Windows ```\Cache``` or ```%Tmp%\Steam++```
    - macOS ```~/Library/Caches/Steam++```
    - Linux ```$XDG_CACHE_HOME/Steam++``` or ```$HOME/.cache/Steam++```
    - Android ```/data/data/net.steampp.app/cache```
- Logs
    - Microsoft Store ```%USERPROFILE%\AppData\Local\Packages\4651ED44255E.47979655102CE_k6txddmbb6c52\LocalCache\Logs```
    - Windows ```\Logs``` or ```%Tmp%\Steam++\Logs```
    - macOS ```~/Library/Caches/Steam++/Logs```
    - Linux ```$XDG_CACHE_HOME/Steam++/Logs``` or ```$HOME/.cache/Steam++/Logs```
    - Android ```/data/data/net.steampp.app/cache/Logs```

### Local authenticator backups

- Before moving to another computer, reinstalling Windows, or switching Windows users, use **Local tokens → Export all**. Choose a backup password and leave **Only this machine can decrypt** off. Keep the backup and its password separately. A copied device-encrypted database is not a portable backup.
- Existing vaults and backups are read in their original formats. In **Token settings**, choose **Upgrade legacy encryption** to re-encrypt eligible old entries after unlocking. Upgrade and password changes update the complete vault in one transaction. New protected entries and exports may not be readable by older releases. Upgrading does not erase old database pages, backups, or exports.
- In **Token settings → Unlock method**, choose **Windows Hello** or **Custom password**. On Windows 11, Hello uses your Windows PIN, fingerprint, or face without a separate vault password. It is an in-app check; Windows DPAPI encrypts the data for the current Windows account. Keep a portable password-protected backup before switching. If Hello is unavailable, a Hello-only vault stays locked; restore Hello or use that backup.
- An older vault that used both Windows Hello and a custom password still needs its original password once. Use **Unlock with existing password**; this ends the old two-step check without changing encryption. Then switch to **Windows Hello** explicitly. Switching re-encrypts the entire vault atomically, does not store the original password, and does not change existing backups. Turning Hello off later leaves Windows account encryption on.