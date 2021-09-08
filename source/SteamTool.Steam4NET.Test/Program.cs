using System;
using System.Threading;

using Steam4NET;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace SteamTool.Steam4NET.Test
{
    class Program
    {
        public static int Main()
        {
            //Environment.SetEnvironmentVariable("SteamAppId", "730");

            Console.Write("Loading Steam2 and Steam3... ");

            if (Steamworks.Load(false))
            {
                Console.WriteLine("Ok");
            }
            else
            {
                Console.WriteLine("Failed");
                return -1;
            }

            Console.WriteLine("\nSteam2 tests:");

            //ISteam006 steam006 = Steamworks.CreateSteamInterface<ISteam006>();
            //if (steam006 == null)
            //{
            //    Console.WriteLine("steam006 is null !");
            //    return -1;
            //}


            TSteamError steamError = new TSteamError();


            //Console.Write("GetVersion: ");
            //StringBuilder version = new StringBuilder();
            //if (steam006.GetVersion(version) != 0)
            //{
            //    Console.WriteLine("Ok (" + version.ToString() + ")");
            //}
            //else
            //{
            //    Console.WriteLine("Failed");
            //    return -1;
            //}

            //steam006.ClearError(ref steamError);

            //Console.Write("Startup: ");
            //if (steam006.Startup(0, ref steamError) != 0)
            //{
            //    Console.WriteLine("Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Failed (" + steamError.szDesc + ")");
            //    return -1;
            //}

            //Console.Write("OpenTmpFile: ");
            //uint hFile;
            //if ((hFile = steam006.OpenTmpFile(ref steamError)) != 0)
            //{
            //    Console.WriteLine("Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Failed (" + steamError.szDesc + ")");
            //    return -1;
            //}

            //Console.Write("WriteFile: ");
            //byte[] fileContent = System.Text.UTF8Encoding.UTF8.GetBytes("test");
            //if (steam006.WriteFile(fileContent, (uint)fileContent.Length, hFile, ref steamError) == fileContent.Length)
            //{
            //    Console.WriteLine("Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Failed (" + steamError.szDesc + ")");
            //    return -1;
            //}

            //Console.Write("CloseFile: ");
            //if (steam006.CloseFile(hFile, ref steamError) == 0)
            //{
            //    Console.WriteLine("Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Failed (" + steamError.szDesc + ")");
            //    return -1;
            //}

            Console.WriteLine("\nSteam3 tests:");

            ISteamClient017 steamclient = Steamworks.CreateInterface<ISteamClient017>();
            //ISteamClient009 steamclient9 = Steamworks.CreateInterface<ISteamClient009>();
            if (steamclient == null)
            {
                Console.WriteLine("steamclient is null !");
                return -1;
            }

            IClientEngine clientengine = Steamworks.CreateInterface<IClientEngine>();
            if (clientengine == null)
            {
                Console.WriteLine("clientengine is null !");
                return -1;
            }

            int pipe = steamclient.CreateSteamPipe();
            if (pipe == 0)
            {
                Console.WriteLine("Failed to create a pipe");
                return -1;
            }

            int user = steamclient.ConnectToGlobalUser(pipe);
            if (user == 0 || user == -1)
            {
                Console.WriteLine("Failed to connect to global user");
                return -1;
            }

            ISteamUser016 steamuser = steamclient.GetISteamUser<ISteamUser016>(user, pipe);
            if (steamuser == null)
            {
                Console.WriteLine("steamuser is null !");
                return -1;
            }
            ISteamUtils005 steamutils = steamclient.GetISteamUtils<ISteamUtils005>(pipe);
            if (steamutils == null)
            {
                Console.WriteLine("steamutils is null !");
                return -1;
            }
            ISteamUserStats002 userstats002 = steamclient.GetISteamUserStats<ISteamUserStats002>(user, pipe);
            if (userstats002 == null)
            {
                Console.WriteLine("userstats002 is null !");
                return -1;
            }
            ISteamUserStats010 userstats010 = steamclient.GetISteamUserStats<ISteamUserStats010>(user, pipe);
            if (userstats010 == null)
            {
                Console.WriteLine("userstats010 is null !");
                return -1;
            }
            IClientUser clientuser = clientengine.GetIClientUser<IClientUser>(user, pipe);
            if (clientuser == null)
            {
                Console.WriteLine("clientuser is null !");
                return -1;
            }

            ISteamApps001 clientapps = steamclient.GetISteamApps<ISteamApps001>(user, pipe);
            if (clientapps == null)
            {
                Console.WriteLine("clientapps is null !");
                return -1;
            }

            ISteamApps006 clientapps6 = steamclient.GetISteamApps<ISteamApps006>(user, pipe);
            if (clientapps6 == null)
            {
                Console.WriteLine("clientapps6 is null !");
                return -1;
            }

            IClientFriends clientfriends = clientengine.GetIClientFriends<IClientFriends>(user, pipe);
            if (clientfriends == null)
            {
                Console.WriteLine("clientfriends is null !");
                return -1;
            }

            //Console.Write("RequestCurrentStats: ");
            //if (userstats002.RequestCurrentStats(steamutils.GetAppID()))
            //{
            //    Console.WriteLine("Ok");
            //}
            //else
            //{
            //    Console.WriteLine("Failed");
            //    return -1;
            //}

            //uint a = steam006.RequestAccountsByEmailAddressEmail("kuku127@msn.com", ref steamError);
            //Console.WriteLine(steamError.nDetailedErrorCode);
            //Console.ReadLine();

            Console.Write("Waiting for stats... ");

            //CallbackMsg_t callbackMsg = new CallbackMsg_t();
            //bool statsReceived = false;
            //while (!statsReceived)
            //{
            //    while (Steamworks.GetCallback(pipe, ref callbackMsg) && !statsReceived)
            //    {
            //        Console.WriteLine(callbackMsg.m_iCallback);
            //        if (callbackMsg.m_iCallback == UserStatsReceived_t.k_iCallback)
            //        {
            //            UserStatsReceived_t userStatsReceived = (UserStatsReceived_t)Marshal.PtrToStructure(callbackMsg.m_pubParam, typeof(UserStatsReceived_t));
            //            if (userStatsReceived.m_steamIDUser == steamuser.GetSteamID() && userStatsReceived.m_nGameID == steamutils.GetAppID())
            //            {
            //                if (userStatsReceived.m_eResult == EResult.k_EResultOK)
            //                {
            //                    Console.WriteLine("Ok");
            //                    statsReceived = true;
            //                }
            //                else
            //                {
            //                    Console.WriteLine("Failed (" + userStatsReceived.m_eResult + ")");
            //                    return -1;
            //                }
            //            }
            //        }
            //        Steamworks.FreeLastCallback(pipe);
            //    }
            //    System.Threading.Thread.Sleep(100);
            //}

            Console.WriteLine("Stats for the current game :");
            uint numStats = userstats002.GetNumStats(steamutils.GetAppID());
            for (uint i = 0; i < numStats; i++)
            {
                string statName = userstats002.GetStatName(steamutils.GetAppID(), i);
                ESteamUserStatType statType = userstats002.GetStatType(steamutils.GetAppID(), statName);
                switch (statType)
                {
                    case ESteamUserStatType.k_ESteamUserStatTypeINT:
                        {
                            int value = 0;
                            Console.Write("\t" + statName + " ");
                            if (userstats002.GetStat(steamutils.GetAppID(), statName, ref value))
                            {
                                Console.WriteLine(value);
                            }
                            else
                            {
                                Console.WriteLine("Failed");
                                return -1;
                            }

                            break;
                        }
                    case ESteamUserStatType.k_ESteamUserStatTypeFLOAT:
                        {
                            float value = 0;
                            Console.Write("\t" + statName + " ");
                            if (userstats002.GetStat(steamutils.GetAppID(), statName, ref value))
                            {
                                Console.WriteLine(value);
                            }
                            else
                            {
                                Console.WriteLine("Failed");
                                return -1;
                            }
                            break;
                        }
                }
            }

            Console.Write("GetNumberOfCurrentPlayers: ");
            ulong getNumberOfCurrentPlayersCall = userstats010.GetNumberOfCurrentPlayers();
            bool failed = false;
            while (!steamutils.IsAPICallCompleted(getNumberOfCurrentPlayersCall, ref failed) && !failed)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (failed)
            {
                Console.WriteLine("Failed (IsAPICallCompleted failure)");
                return -1;
            }

            IntPtr pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NumberOfCurrentPlayers_t)));

            if (!Steamworks.GetAPICallResult(pipe, getNumberOfCurrentPlayersCall, pData, Marshal.SizeOf(typeof(NumberOfCurrentPlayers_t)), NumberOfCurrentPlayers_t.k_iCallback, ref failed))
            {
                Console.WriteLine("Failed (GetAPICallResult failure: " + steamutils.GetAPICallFailureReason(getNumberOfCurrentPlayersCall) + ")");
                return -1;
            }

            NumberOfCurrentPlayers_t numberOfCurrentPlayers = (NumberOfCurrentPlayers_t)Marshal.PtrToStructure(pData, typeof(NumberOfCurrentPlayers_t));
            if (!System.Convert.ToBoolean(numberOfCurrentPlayers.m_bSuccess))
            {
                Console.WriteLine("Failed (numberOfCurrentPlayers.m_bSuccess is false)");
                return -1;
            }
            Console.WriteLine("Ok (" + numberOfCurrentPlayers.m_cPlayers + ")");

            Marshal.FreeHGlobal(pData);


            Console.Write("Games running: ");
            for(int i = 0; i < clientuser.NumGamesRunning(); i++)
            {
                CGameID gameID = clientuser.GetRunningGameID(i);
                Console.Write(gameID);
                if(i + 1 < clientuser.NumGamesRunning())
                    Console.Write(", ");
                else
                    Console.Write("\n");
            }

            Console.ReadKey();

            FriendSessionStateInfo_t sessionStateInfo = clientfriends.GetFriendSessionStateInfo(clientuser.GetSteamID());

            clientfriends.SetPersonaState(EPersonaState.k_EPersonaStateAway);

            Console.WriteLine("m_uiOnlineSessionInstances: " + sessionStateInfo.m_uiOnlineSessionInstances);
            Console.WriteLine("m_uiPublishedToFriendsSessionInstance: " + sessionStateInfo.m_uiPublishedToFriendsSessionInstance);

            Console.Write("RequestFriendProfileInfo: ");
            ulong requestFriendProfileInfoCall = clientfriends.RequestFriendProfileInfo(steamuser.GetSteamID());

            while (!steamutils.IsAPICallCompleted(requestFriendProfileInfoCall, ref failed) && !failed)
            {
                System.Threading.Thread.Sleep(100);
            }

            if (failed)
            {
                Console.WriteLine("Failed (IsAPICallCompleted failure)");
                return -1;
            }

            pData = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FriendProfileInfoResponse_t)));

            if (!Steamworks.GetAPICallResult(pipe, requestFriendProfileInfoCall, pData, Marshal.SizeOf(typeof(FriendProfileInfoResponse_t)), FriendProfileInfoResponse_t.k_iCallback, ref failed))
            {
                Console.WriteLine("Failed (GetAPICallResult failure: " + steamutils.GetAPICallFailureReason(requestFriendProfileInfoCall) + ")");
                return -1;
            }

            FriendProfileInfoResponse_t friendProfileInfoResponse = (FriendProfileInfoResponse_t)Marshal.PtrToStructure(pData, typeof(FriendProfileInfoResponse_t));
            if (friendProfileInfoResponse.m_eResult != EResult.k_EResultOK)
            {
                Console.WriteLine("Failed (friendProfileInfoResponse.m_eResult = " + friendProfileInfoResponse.m_eResult + ")");
                return -1;
            }
            if (friendProfileInfoResponse.m_steamIDFriend == clientuser.GetSteamID())
            {
                Console.WriteLine("Ok");
            }
            else
            {
                Console.WriteLine("Failed (SteamIDs doesn't match)");
            }

            Marshal.FreeHGlobal(pData);

            Console.ReadKey();
            return 0;
        }
    }
}
