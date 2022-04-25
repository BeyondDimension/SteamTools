using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace System.Application.Models
{
    partial class MyAuthenticator
    {
        public static string CodeFormat(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return string.Empty;
            }
            var c3 = code.Length / 3;
            if (code.Length % 3 == 0 && c3 > 1)
            {
                var list = code.ToCharArray().ToList();
                for (int i = 1; i < c3; i++)
                {
                    list.Insert(i * 3, ' ');
                }
                return new string(list.ToArray());
            }
            else
            {
                var arr = code.ToCharArray();
                return string.Join(' ', arr);
            }
        }

        int _AutoRefreshCodeTimingCurrent = -1;

        /// <summary>
        /// 当前自动刷新倒计时值
        /// </summary>
        public int AutoRefreshCodeTimingCurrent
        {
            get => _AutoRefreshCodeTimingCurrent;
            set => this.RaiseAndSetIfChanged(ref _AutoRefreshCodeTimingCurrent, value);
        }

        int TimingCycle
        {
            get
            {
                var value = AuthenticatorData.Value.ServerTime % Convert.ToInt64(TimeSpan.FromSeconds(Period).TotalMilliseconds);
                return Period - Convert.ToInt32(TimeSpan.FromMilliseconds(value).TotalSeconds);
            }
        }

        /// <summary>
        /// 自动刷新一次性密码代码
        /// </summary>
        public interface IAutoRefreshCodeHost
        {
            /// <summary>
            /// 当前自动刷新的 <see cref="System.Threading.Timer"/>
            /// </summary>
            protected Timer? Timer { get; set; }

            /// <summary>
            /// 获取当前正在显示中的视图模型组
            /// </summary>
            protected IEnumerable<MyAuthenticator> ViewModels { get; }

            /// <summary>
            /// 停止当前自动刷新一次性密码代码
            /// </summary>
            void StopTimer()
            {
                if (Timer != null)
                {
                    Timer?.Dispose();
                    Timer = null;
                }
            }

            /// <summary>
            /// 开始自动刷新一次性密码代码
            /// </summary>
            void StartTimer()
            {
                if (Timer != null) return;
                Timer = new((_) =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    foreach (var item in ViewModels)
                    {
                        var value = item.TimingCycle;
                        var isZero = value <= 0;
                        if (isZero) value = item.Period;
#if DEBUG
                        Log.Debug("AutoRefreshCode", "while({1}), name: {0}", item.Name, value);
#endif
                        string? code = item.GetNextCode();
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            item.AutoRefreshCodeTimingCurrent = value;
                            if (code != null)
                            {
                                item.RefreshCode(code);
                            }
                        });
                    }
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }
        }
    }
}
