/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Runtime.InteropServices;

namespace SAM.API.Interfaces
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ISteamApps008
    {
        public IntPtr IsSubscribed;
        public IntPtr IsLowViolence;
        public IntPtr IsCybercafe;
        public IntPtr IsVACBanned;
        public IntPtr GetCurrentGameLanguage;
        public IntPtr GetAvailableGameLanguages;
        public IntPtr IsSubscribedApp;
        public IntPtr IsDlcInstalled;
        public IntPtr GetEarliestPurchaseUnixTime;
        public IntPtr IsSubscribedFromFreeWeekend;
        public IntPtr GetDLCCount;
        public IntPtr GetDLCDataByIndex;
        public IntPtr InstallDLC;
        public IntPtr UninstallDLC;
        public IntPtr RequestAppProofOfPurchaseKey;
        public IntPtr GetCurrentBetaName;
        public IntPtr MarkContentCorrupt;
        public IntPtr GetInstalledDepots;
        public IntPtr GetAppInstallDir;
        public IntPtr IsAppInstalled;
        public IntPtr GetAppOwner;
        public IntPtr GetLaunchQueryParam;
        public IntPtr GetDlcDownloadProgress;
        public IntPtr GetAppBuildId;
        public IntPtr RequestAllProofOfPurchaseKeys;
        public IntPtr GetFileDetails;
        public IntPtr GetLaunchCommandLine;
        public IntPtr IsSubscribedFromFamilySharing;
    }
}
