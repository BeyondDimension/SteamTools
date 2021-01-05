using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;

namespace MetroTrilithon.Desktop
{
	/// <summary>
	/// アプリケーションの多重起動の検知と、起動しているインスタンス間でのメッセージの送受信をサポートします。
	/// </summary>
	public sealed class ApplicationInstance : IDisposable
	{
		private readonly IChannel _channel;
		private readonly ApplicationInstanceMessage _appInstanceMessage;

		/// <summary>
		/// このインスタンスが、初回起動のインスタンスかどうかを示す値を取得します。
		/// </summary>
		public bool IsFirst { get; }

		/// <summary>
		/// 新たに起動されたインスタンスのコマンドラインを受信した時に発生します。
		/// </summary>
		public event EventHandler<MessageEventArgs> CommandLineArgsReceived;

		public ApplicationInstance() : this(Assembly.GetEntryAssembly()) { }

		public ApplicationInstance(Assembly targetAssembly)
		{
			// アプリケーションの GUID を取得
			var portName = ((GuidAttribute)Attribute.GetCustomAttribute(targetAssembly, typeof(GuidAttribute))).Value;

			// セッションごとの URI となるように、セッション ID を URI として使用
			var uri = Process.GetCurrentProcess().SessionId.ToString(CultureInfo.InvariantCulture);

			// IPC サーバーの作成に成功した場合、指定したポートを使用する IPC 通信が初めて作成されたものとする
			// 作成に失敗した場合、別のインスタンスが IPC サーバーを作成しているので、クライアントを作成
			// で、これらの処理が同時に発生しないよう、Mutex で保護
			using (var mutex = new Mutex(true, typeof(ApplicationInstanceMessage).FullName + "_" + portName))
			{
				mutex.WaitOne();
				try
				{
					// サーバーを作成
					this._channel = new IpcServerChannel(portName, portName, new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full, });
					ChannelServices.RegisterChannel(this._channel, true);

					RemotingConfiguration.RegisterWellKnownServiceType(typeof(ApplicationInstanceMessage), uri, WellKnownObjectMode.Singleton);
					this._appInstanceMessage = new ApplicationInstanceMessage();
					RemotingServices.Marshal(this._appInstanceMessage, uri, typeof(ApplicationInstanceMessage));

					// ApplicationInstanceServiceMessage の CommandLineArgsReceived イベントを購読して、
					// このクラスに設定された CommandLineArgsReceived イベントを発生させる
					this._appInstanceMessage.MessageReceived += this.OnMessageReceived;

					this.IsFirst = true;
				}
				catch (RemotingException)
				{
					// クライアント作成
					this._channel = new IpcClientChannel();
					ChannelServices.RegisterChannel(this._channel, true);

					RemotingConfiguration.RegisterWellKnownClientType(typeof(ApplicationInstanceMessage), $"ipc://{portName}/{uri}");

					this._appInstanceMessage = new ApplicationInstanceMessage();

					this.IsFirst = false;
				}
			}
		}

		/// <summary>
		/// <see cref="CommandLineArgsReceived"/> イベントを発生させます。
		/// </summary>
		/// <param name="commandLineArgs">現在のプロセスのコマンドライン引数。</param>
		private void OnMessageReceived(string[] commandLineArgs)
		{
			this.CommandLineArgsReceived?.Invoke(this, new MessageEventArgs(commandLineArgs));
		}

		/// <summary>
		/// 既に存在するインスタンスに対して、現在のプロセスのコマンド ライン引数を送信します。
		/// </summary>
		/// <param name="commandLineArgs">現在のプロセスのコマンド ライン引数。</param>
		public void SendCommandLineArgs(string[] commandLineArgs)
		{
			this._appInstanceMessage.OnMessageReceived(commandLineArgs);
		}

		public void Dispose()
		{
			ChannelServices.UnregisterChannel(this._channel);
		}


		/// <summary>
		/// すべてのプロセスが共通して持ち、送受信されるメッセージを格納します。
		/// </summary>
		private class ApplicationInstanceMessage : MarshalByRefObject
		{
			/// <summary>
			/// メッセージを受信したときに発生します。
			/// </summary>
			public event Action<string[]> MessageReceived;

			/// <summary>
			/// <see cref="MessageReceived"/> イベントを発生させます。
			/// </summary>
			public void OnMessageReceived(string[] commandLineArgs)
			{
				this.MessageReceived?.Invoke(commandLineArgs);
			}

			public override object InitializeLifetimeService()
			{
				// このオブジェクトのリース期限を無制限にするやつ
				// これをやらないと、オブジェクトにアクセスせず一定期限が過ぎると自動的に破棄されるかもしれない
				var lease = base.InitializeLifetimeService() as ILease;
				if (lease != null && lease.CurrentState == LeaseState.Initial)
				{
					lease.InitialLeaseTime = TimeSpan.Zero;
				}

				return lease;
			}
		}
	}


	/// <summary>
	/// プロセス間通信で発生するメッセージの受け渡しに関するイベントのデータを提供します。
	/// </summary>
	public class MessageEventArgs : EventArgs
	{
		/// <summary>
		/// メッセージを取得します。
		/// </summary>
		public string[] CommandLineArgs { get; }

		public MessageEventArgs(string[] commandLineArgs)
		{
			this.CommandLineArgs = commandLineArgs;
		}
	}
}
