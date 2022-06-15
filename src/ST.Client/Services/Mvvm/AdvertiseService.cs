using System;
using System.Collections.Generic;
using System.Text;
using System.Application.Services;

namespace System.Application.Services.Mvvm
{
    public class AdvertiseService
    {
        static AdvertiseService? mCurrent;

        public static AdvertiseService Current => mCurrent ?? new();
        
        readonly I archiSteamFarmService = IArchiSteamFarmService.Instance;

        private AdvertiseService()
        {

        }

        public void InitAdvertise()
        {

        }
    }
}
