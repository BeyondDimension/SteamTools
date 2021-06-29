using ReactiveUI;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IArchiSteamFarmService
    {
        public static IArchiSteamFarmService Instance => DI.Get<IArchiSteamFarmService>();


        /// <summary>
        /// 启动ASF
        /// </summary>
        /// <param name="args"></param>
        void Start(string[]? args = null);

        /// <summary>
        /// 执行asf指令
        /// </summary>
        /// <param name="command"></param>
        Task<string?> ExecuteCommand(string command);
    }
}