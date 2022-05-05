using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Properties;

namespace System
{
    public static class Process2
    {
        const string TAG = "Process2";

        static readonly Lazy<string> _bin_bash = new(() => string.Format("{0}bin{0}bash", Path.DirectorySeparatorChar));

        /// <summary>
        /// /bin/bash
        /// </summary>
        public static string BinBash => _bin_bash.Value;

        /// <summary>
        /// 通过指定应用程序的名称和路径参数来启动一个进程资源，并将该资源与新的 Process 组件相关联
        /// </summary>
        /// <param name="fileName">要在进程中运行的应用程序文件的名称</param>
        /// <param name="path">要传递的路径</param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">已释放此进程对象</exception>
        /// <exception cref="Win32Exception">
        /// <para>打开关联的文件时出错</para>
        /// <para>或 fileName 中指定的文件未找到</para>
        /// <para>或 参数的长度与该进程的完整路径的长度的总和超过了 2080。 与此异常关联的错误消息可能为以下消息之一：“传递到系统调用的数据区域太小。” 或“拒绝访问。”</para>
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">不支持 shell 的操作系统（如，仅适用于.NET Core 的 Nano Server）不支持此方法</exception>
        public static Process? StartPath(string fileName, string path)
        {
            if (OperatingSystem2.IsMacOS)
            {
                var arguments = $"-a \"{fileName}\" \"{path}\"";
                using var process = Start("open", arguments, true);
                if (process == null)
                {
                    Log.Error(TAG, "StartPath(macOS) return null, fileName:{0}, path: {1}",
                        fileName, path);
                    return null;
                }
                process.Close();
                return process;
            }
            else
            {
                return Start(fileName, $"\"{path}\"", false);
            }
        }

        /// <summary>
        /// 通过指定应用程序的名称和一组命令行参数来启动一个进程资源，并将该资源与新的 Process 组件相关联
        /// </summary>
        /// <param name="fileName">要在进程中运行的应用程序文件的名称</param>
        /// <param name="arguments">启动该进程时传递的命令行参数</param>
        /// <param name="useShellExecute">获取或设置指示是否使用操作系统 shell 启动进程的值</param>
        /// <param name="workingDirectory">当 useShellExecute 属性为 <see langword="false"/> 时，将获取或设置要启动的进程的工作目录。 当 useShellExecute 为 <see langword="true"/> 时，获取或设置包含要启动的进程的目录<para>注意：当 UseShellExecute 为  <see langword="true"/> 时，是包含要启动的进程的目录的完全限定名</para></param>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">已释放此进程对象</exception>
        /// <exception cref="Win32Exception">
        /// <para>打开关联的文件时出错</para>
        /// <para>或 fileName 中指定的文件未找到</para>
        /// <para>或 参数的长度与该进程的完整路径的长度的总和超过了 2080。 与此异常关联的错误消息可能为以下消息之一：“传递到系统调用的数据区域太小。” 或“拒绝访问。”</para>
        /// </exception>
        /// <exception cref="PlatformNotSupportedException">不支持 shell 的操作系统（如，仅适用于.NET Core 的 Nano Server）不支持此方法</exception>
        public static Process? Start(string fileName, string? arguments = null, bool useShellExecute = false, string? workingDirectory = null, IReadOnlyDictionary<string, string>? environment = null)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            var p = new ProcessStartInfo(fileName);
            if (!string.IsNullOrEmpty(arguments))
            {
                p.Arguments = arguments;
            }
            if (environment != null)
            {
                foreach (var item in environment)
                {
                    p.Environment.Add(item.Key, item.Value);
                }
            }
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                p.WorkingDirectory = workingDirectory;
            }
            else if (!useShellExecute && fileName.Contains(Path.DirectorySeparatorChar))
            {
                FileInfo fileInfo = new(fileName);
                if (fileInfo.Exists && !string.IsNullOrEmpty(fileInfo.DirectoryName))
                {
                    p.WorkingDirectory = fileInfo.DirectoryName;
                }
            }
            p.UseShellExecute = useShellExecute;
            return Process.Start(p);
        }

        public static bool OpenCoreByProcess(string url, Action<string>? onError = null)
        {
            try
            {
                Start(url, useShellExecute: true);
                return true;
            }
            catch (Win32Exception e)
            {
                // [Win32Exception: 找不到应用程序] 39次报告
                // 疑似缺失没有默认浏览器设置会导致此异常，可能与杀毒软件有关
                onError?.Invoke(SR.OpenCoreByProcess_Win32Exception_.Format(Convert.ToString(e.NativeErrorCode, 16)));
                return false;
            }
        }
    }
}