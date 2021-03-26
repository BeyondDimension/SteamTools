using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;

namespace System.Application.UI
{
    /// <summary>
    /// 支持检测应用程序的多次启动以及在正在运行的实例之间发送和接收消息。
    /// </summary>
    public sealed class ApplicationInstance
    {
        private readonly ApplicationInstanceMessage _appInstanceMessage;

        /// <summary>
        /// 获取一个值，该值指示此实例是否是第一个要启动的实例。
        /// </summary>
        public bool IsFirst { get; }

        /// <summary>
        /// 在接收新启动的实例的命令行时发生。
        /// </summary>
        public event EventHandler<MessageEventArgs> CommandLineArgsReceived;

        public ApplicationInstance() : this(Assembly.GetEntryAssembly()) { }

        public ApplicationInstance(Assembly targetAssembly)
        {
            // 获取应用程序的GUID
            var portName = ((GuidAttribute)Attribute.GetCustomAttribute(targetAssembly, typeof(GuidAttribute))).Value;

            // 使用会话ID作为URI，以便它是每个会话URI
            //var uri = Process.GetCurrentProcess().SessionId.ToString(CultureInfo.InvariantCulture);

            // 如果成功创建了IPC服务器，则假定是首次创建使用指定端口的IPC通信。
            // 如果创建失败，则说明另一个实例正在创建IPC服务器，因此在创建客户端时，Mutex会同时保护其免受这些操作的影响。
            using var mutex = new Mutex(true, typeof(ApplicationInstanceMessage).FullName + "_" + portName, out bool flag);
            mutex.WaitOne();
            this.IsFirst = flag;
        }

        /// <summary>
        /// <see cref="CommandLineArgsReceived"/> 引发一个事件。
        /// </summary>
        /// <param name="commandLineArgs">当前进程的命令行参数。</param>
        private void OnMessageReceived(string[] commandLineArgs)
        {
            this.CommandLineArgsReceived?.Invoke(this, new MessageEventArgs(commandLineArgs));
        }

        /// <summary>
        /// 将当前进程的命令行参数发送到已经存在的实例。
        /// </summary>
        /// <param name="commandLineArgs">当前进程的命令行参数。</param>
        public void SendCommandLineArgs(string[] commandLineArgs)
        {
            this._appInstanceMessage.OnMessageReceived(commandLineArgs);
        }


        /// <summary>
        /// 存储所有进程共有的消息以及已发送和接收的消息。
        /// </summary>
        private class ApplicationInstanceMessage : MarshalByRefObject
        {
            /// <summary>
            /// 收到消息时发生。
            /// </summary>
            public event Action<string[]> MessageReceived;

            /// <summary>
            /// <see cref="MessageReceived"/> 引发一个事件。
            /// </summary>
            public void OnMessageReceived(string[] commandLineArgs)
            {
                this.MessageReceived?.Invoke(commandLineArgs);
            }
        }
    }

    /// <summary>
    /// 提供有关与进程间通信中发生的消息传递有关的事件的数据。
    /// </summary>
    public class MessageEventArgs : EventArgs
    {
        /// <summary>
        /// 获取消息。
        /// </summary>
        public string[] CommandLineArgs { get; }

        public MessageEventArgs(string[] commandLineArgs)
        {
            this.CommandLineArgs = commandLineArgs;
        }
    }
}
