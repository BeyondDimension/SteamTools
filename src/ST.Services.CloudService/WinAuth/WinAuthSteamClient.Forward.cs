using System;
using System.Collections.Generic;
using System.Text;
using static System.Application.ForwardHelper;

namespace WinAuth
{
    partial class WinAuthSteamClient
    {
        static string TryGetForwardUrl(string url)
        {
            if (IsAllowUrl(url))
            {

            }
            return url;
        }
    }
}
