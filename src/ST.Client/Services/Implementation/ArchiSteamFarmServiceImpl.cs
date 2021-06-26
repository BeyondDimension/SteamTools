using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Services.Implementation
{
    public class ArchiSteamFarmServiceImpl : IArchiSteamFarmService
    {
        public async void Start(string[]? args = null)
        {
            try
            {
                await ArchiSteamFarm.Program.Init(args).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Toast.Show(ex.Message);
            }
        }
    }
}
