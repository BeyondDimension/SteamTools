using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface ISteamworksLocalApiService
    {
        //Client SteamClient { get; }
    }


#if DEBUG

    [Obsolete("use ISteamworksLocalApiService", true)]
    public class SteamworksApiService
    {

    }

#endif
}
