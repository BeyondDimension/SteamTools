using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.UI
{
    /// <summary>
    /// 支持检测应用程序的多次启动以及在正在运行的实例之间发送和接收消息
    /// </summary>
    public sealed class ApplicationInstance : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// 获取一个值，该值指示此实例是否是第一个要启动的实例
        /// </summary>
        public bool IsFirst { get; }

        /// <summary>
        /// 在接收新启动的实例的消息时发生
        /// </summary>
        public event Action<string>? MessageReceived;

        public ApplicationInstance()
        {
            IsFirst = GetIsFirst();
            if (IsFirst)
            {
                Task.Run(RunPipeServer);
            }
        }

        static IEnumerable<Process> GetCurrentAllProcess()
        {
            var current = Process.GetCurrentProcess();
#if DEBUG
            if (string.Equals("dotnet", Path.GetFileNameWithoutExtension(current.MainModule?.FileName), StringComparison.OrdinalIgnoreCase))
            {
                return Array.Empty<Process>();
            }
#endif
            var currentMainModule = current.TryGetMainModule();
            var query = from p in Process.GetProcessesByName(current.ProcessName)
                        let pMainModule = p.TryGetMainModule()
                        where p.Id != current.Id &&
                            p.ProcessName == current.ProcessName &&
                            (currentMainModule == null ||
                                (pMainModule != null &&
                                    pMainModule.FileName == currentMainModule.FileName &&
                                        pMainModule.ModuleName == currentMainModule.ModuleName))
                        select p;
            return query;
        }

        public static bool TryKillCurrentAllProcess()
        {
            foreach (var p in GetCurrentAllProcess())
            {
                try
                {
                    p.Kill(true);
                    p.WaitForExit(500);
                }
                catch
                {
                }
            }
            return GetIsFirst();
        }

        static bool GetIsFirst()
        {
            var query = GetCurrentAllProcess();
            var r = query.Any();
            return !r;
        }

        #region Pipes

        static string GetPipeName() => BuildConfig.APPLICATION_ID + "_" + Hashs.String.Crc32(AppHelper.ProgramPath);

        CancellationTokenSource? cts;
        void RunPipeServer()
        {
            cts = new CancellationTokenSource();
            var name = GetPipeName();
            while (true)
            {
                using var pipeServer = new NamedPipeServerStream(name, PipeDirection.In, 1);
                try
                {
                    pipeServer.WaitForConnection();
                    using var sr = new StreamReader(pipeServer);
                    //int i = 0;
                    try
                    {
                        var line = sr.ReadLine();
                        //Console.WriteLine($"({i++})RunPipeServer line: {line ?? "null"}, name: {name}");
                        if (line != null)
                        {
                            MessageReceived?.Invoke(line);
                        }
                    }
                    catch (IOException)
                    {
                    }
                    pipeServer.Close();
                    cts.Token.ThrowIfCancellationRequested();
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Log.Error(nameof(ApplicationInstance), e, nameof(RunPipeServer));
                    return;
                }
            }
        }

        /// <summary>
        /// 给主进程发送消息
        /// </summary>
        /// <param name="value"></param>
        public static bool SendMessage(string value)
        {
            var name = GetPipeName();
            try
            {
                using var pipeClient = new NamedPipeClientStream(".", name, PipeDirection.Out, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                pipeClient.Connect(1000);
                using StreamWriter sw = new StreamWriter(pipeClient);
                sw.WriteLine(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    cts?.Cancel();
                    MessageReceived = null;
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ApplicationInstance()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}