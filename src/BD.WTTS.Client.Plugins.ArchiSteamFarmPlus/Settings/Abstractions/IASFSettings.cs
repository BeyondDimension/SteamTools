using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Settings.Abstractions;

public partial interface IASFSettings
{
    static IASFSettings? Instance
    => Ioc.Get_Nullable<IOptionsMonitor<IASFSettings>>()?.CurrentValue;

    string ArchiSteamFarmExePath { get; set; }

    bool AutoRunArchiSteamFarm { get; set; }

    bool CheckArchiSteamFarmExe { get; set; }

    #region ConsoleMaxLine

    /// <summary>
    /// 控制台默认最大行数
    /// </summary>
    const int DefaultConsoleMaxLine = 200;

    /// <summary>
    /// 控制台默认最大行数范围最小值
    /// </summary>
    const int MinRangeConsoleMaxLine = DefaultConsoleMaxLine;

    /// <summary>
    /// 控制台默认最大行数范围最大值
    /// </summary>
    const int MaxRangeConsoleMaxLine = 5000;

    int ConsoleMaxLine { get; set; }

    #endregion

    const int DefaultConsoleFontSize = 14;
    const int MinRangeConsoleFontSize = 8;
    const int MaxRangeConsoleFontSize = 24;

    int ConsoleFontSize { get; set; }

    const int DefaultIPCPortIdValue = 6242;

    int IPCPortId { get; set; }

    bool IPCPortOccupiedRandom { get; set; }
}
