using System.Globalization;

namespace System.Application
{
    /// <summary>
    /// 统一控制台输出头部内容
    /// </summary>
    internal static class UnityConsoleOutputHead
    {
        /// <inheritdoc cref="UnityConsoleOutputHead"/>
        public static void Write(Action<string> writeLine,
            string projectName,
            string? version,
            string? runtimeVersion = null,
            string? cpuName = null)
        {
            // 项目代号和版本信息
            writeLine($"Project {projectName} [{nameof(Version)} {version}{(string.IsNullOrEmpty(runtimeVersion) ? null : $" / Runtime {runtimeVersion}")}]" + Environment.NewLine);
            if (!string.IsNullOrEmpty(cpuName))
                writeLine($"CentralProcessorName: {cpuName} x{Environment.ProcessorCount}");
            writeLine($"LocalTime: {DateTimeOffset.Now.ToLocalTime()}");
            // 输出当前系统设置区域
            writeLine($"CurrentCulture: {CultureInfo.CurrentCulture.Name} {CultureInfo.CurrentCulture.EnglishName}");
            writeLine(string.Empty);
        }
    }
}