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
    public struct ISteamClient018
    {
        public IntPtr CreateSteamPipe;
        public IntPtr ReleaseSteamPipe;
        public IntPtr ConnectToGlobalUser;
        public IntPtr CreateLocalUser;
        public IntPtr ReleaseUser;
        public IntPtr GetISteamUser;
        public IntPtr GetISteamGameServer;
        public IntPtr SetLocalIPBinding;
        public IntPtr GetISteamFriends;
        public IntPtr GetISteamUtils;
        public IntPtr GetISteamMatchmaking;
        public IntPtr GetISteamMatchmakingServers;
        public IntPtr GetISteamGenericInterface;
        public IntPtr GetISteamUserStats;
        public IntPtr GetISteamGameServerStats;
        public IntPtr GetISteamApps;
        public IntPtr GetISteamNetworking;
        public IntPtr GetISteamRemoteStorage;
        public IntPtr GetISteamScreenshots;
        public IntPtr GetISteamGameSearch;
        public IntPtr RunFrame;
        public IntPtr GetIPCCallCount;
        public IntPtr SetWarningMessageHook;
        public IntPtr ShutdownIfAllPipesClosed;
        public IntPtr GetISteamHTTP;
        public IntPtr DEPRECATED_GetISteamUnifiedMessages;
        public IntPtr GetISteamController;
        public IntPtr GetISteamUGC;
        public IntPtr GetISteamAppList;
        public IntPtr GetISteamMusic;
        public IntPtr GetISteamMusicRemote;
        public IntPtr GetISteamHTMLSurface;
        public IntPtr DEPRECATED_Set_SteamAPI_CPostAPIResultInProcess;
        public IntPtr DEPRECATED_Remove_SteamAPI_CPostAPIResultInProcess;
        public IntPtr Set_SteamAPI_CCheckCallbackRegisteredInProcess;
        public IntPtr GetISteamInventory;
        public IntPtr GetISteamVideo;
        public IntPtr GetISteamParentalSettings;
        public IntPtr GetISteamInput;
        public IntPtr GetISteamParties;
    }
}
