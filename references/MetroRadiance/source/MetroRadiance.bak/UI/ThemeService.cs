using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MetroRadiance.Interop.Win32;
using MetroRadiance.Media;
using MetroRadiance.Platform;
using MetroRadiance.Utilities;

namespace MetroRadiance.UI
{
	/// <summary>
	/// MetroRadiance テーマ機能を提供します。
	/// </summary>
	public class ThemeService : INotifyPropertyChanged
	{
		#region singleton members

		public static ThemeService Current { get; } = new ThemeService();

		#endregion

		private static readonly UriTemplate themeTemplate = new UriTemplate(@"Themes/{theme}.xaml");
		private static readonly UriTemplate accentTemplate = new UriTemplate(@"Themes/Accents/{accent}.xaml");
		private static readonly Uri templateBaseUri = new Uri(@"pack://application:,,,/MetroRadiance;component");

		private Dispatcher dispatcher;
		private IDisposable windowsAccentListener;
		private IDisposable windowsThemeListener;

		private readonly List<ResourceDictionary> themeResources = new List<ResourceDictionary>();
		private readonly List<ResourceDictionary> accentResources = new List<ResourceDictionary>();

		#region Theme 変更通知プロパティ

		private Theme? _Theme;

		/// <summary>
		/// 現在設定されているテーマを取得します。
		/// </summary>
		public Theme Theme
		{
			get { return this._Theme ?? Theme.Windows; }
			private set
			{
				if (this._Theme != value)
				{
					this._Theme = value;
					this.UpdateListener(value);
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Accent 変更通知プロパティ

		private Accent? _Accent;

		/// <summary>
		/// 現在設定されているアクセントを取得します。
		/// </summary>
		public Accent Accent
		{
			get { return this._Accent ?? Accent.Windows; }
			private set
			{
				if (this._Accent != value)
				{
					this._Accent = value;
					this.UpdateListener(value);
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		private ThemeService() { }

		/// <summary>
		/// テーマ機能を有効化します。テーマまたはアクセントが変更されたとき、<paramref name="app"/>
		/// で指定した WPF アプリケーション内のテーマ関連リソースは自動的に書き換えられます。
		/// </summary>
		/// <param name="app">テーマ関連リソースを含む WPF アプリケーション。</param>
		/// <param name="theme">初期値として使用するテーマ。</param>
		/// <param name="accent">初期値として使用するアクセント。</param>
		/// <returns><paramref name="app"/> をリソースの書き換え対象から外すときに使用する <see cref="IDisposable"/> オブジェクト。</returns>
		public IDisposable Register(Application app, Theme theme, Accent accent)
		{
			this.dispatcher = app.Dispatcher;

			var disposable = this.Register(app.Resources, theme, accent);

			this.Theme = theme;
			this.Accent = accent;

			return disposable;
		}

		/// <summary>
		/// テーマまたはアクセントが変更されたときにリソースの書き換え対象とする <see cref="ResourceDictionary"/>
		/// を登録します。このメソッドは、登録解除に使用する <see cref="IDisposable"/> オブジェクトを返します。
		/// </summary>
		/// <returns><paramref name="rd"/> をリソースの書き換え対象から外すときに使用する <see cref="IDisposable"/> オブジェクト。</returns>
		public IDisposable Register(ResourceDictionary rd)
		{
			return this.Register(rd, this.Theme, this.Accent);
		}

		internal IDisposable Register(ResourceDictionary rd, Theme theme, Accent accent)
		{
			this.SetAppMode(theme);

			var allDictionaries = EnumerateDictionaries(rd).ToArray();

			var themeDic = GetThemeResource(theme);
			var targetThemeDic = allDictionaries.FirstOrDefault(x => CheckThemeResourceUri(x.Source));
			if (targetThemeDic == null)
			{
				targetThemeDic = themeDic;
				rd.MergedDictionaries.Add(targetThemeDic);
			}
			else
			{
				foreach (var key in themeDic.Keys.OfType<string>().Where(x => targetThemeDic.Contains(x)))
				{
					targetThemeDic[key] = themeDic[key];
				}
			}
			this.themeResources.Add(targetThemeDic);

			var accentDic = GetAccentResource(accent);
			var targetAccentDic = allDictionaries.FirstOrDefault(x => CheckAccentResourceUri(x.Source));
			if (targetAccentDic == null)
			{
				targetAccentDic = accentDic;
				rd.MergedDictionaries.Add(targetAccentDic);
			}
			else
			{
				foreach (var key in accentDic.Keys.OfType<string>().Where(x => targetAccentDic.Contains(x)))
				{
					targetAccentDic[key] = accentDic[key];
				}
			}
			this.accentResources.Add(targetAccentDic);

			// Unregister したいときは戻り値の IDisposable を Dispose() してほしい
			return Disposable.Create(() =>
			{
				this.themeResources.Remove(targetThemeDic);
				this.accentResources.Remove(targetAccentDic);
			});
		}

		public void ChangeTheme(Theme theme)
		{
			if (this.Theme == theme) return;

			this.InvokeOnUIDispatcher(() => this.ChangeThemeCore(theme));
			this.Theme = theme;
		}

		private void ChangeThemeCore(Platform.Theme theme)
		{
			switch (theme)
			{
				case Platform.Theme.Dark:
					this.ChangeThemeCore(Theme.Dark);
					break;
				case Platform.Theme.Light:
					this.ChangeThemeCore(Theme.Light);
					break;
			}
		}

		private void ChangeThemeCore(Theme theme)
		{
			this.SetAppMode(theme);

			var dic = GetThemeResource(theme);

			foreach (var key in dic.Keys.OfType<string>())
			{
				foreach (var resource in this.themeResources.Where(x => x.Contains(key)))
				{
					resource[key] = dic[key];
				}
			}
		}

		private bool SetAppMode(Theme theme)
		{
			PreferredAppMode appMode;
			if (theme.SyncToWindows)
			{
				appMode = PreferredAppMode.APPMODE_ALLOWDARK;
			}
			else if (theme.Specified == Theme.SpecifiedColor.Dark)
			{
				appMode = PreferredAppMode.APPMODE_FORCEDARK;
			}
			else
			{
				appMode = PreferredAppMode.APPMODE_DEFAULT;
			}
			return AppMode.SetAppMode(appMode);
		}

		public void ChangeAccent(Accent accent)
		{
			if (this.Accent == accent) return;

			this.InvokeOnUIDispatcher(() => this.ChangeAccentCore(accent));
			this.Accent = accent;
		}

		private void ChangeAccentCore(Accent accent)
		{
			this.ChangeAccentCore(GetAccentResource(accent));
		}

		private void ChangeAccentCore(Color color)
		{
			this.ChangeAccentCore(GetAccentResource(color));
		}

		private void ChangeAccentCore(ResourceDictionary dic)
		{
			foreach (var key in dic.Keys.OfType<string>())
			{
				foreach (var resource in this.accentResources.Where(x => x.Contains(key)))
				{
					resource[key] = dic[key];
				}
			}
		}

		private static ResourceDictionary GetThemeResource(Theme theme)
		{
			var specified = theme.SyncToWindows
				? WindowsTheme.Theme.Current == Platform.Theme.Dark ? Theme.Dark.Specified : Theme.Light.Specified
				: theme.Specified;
			if (specified == null) throw new ArgumentException($"Invalid theme value '{theme}'.");

			return new ResourceDictionary { Source = CreateThemeResourceUri(specified.Value), };
		}

		private static ResourceDictionary GetAccentResource(Accent accent)
		{
			return accent.Specified != null
				? new ResourceDictionary { Source = CreateAccentResourceUri(accent.Specified.Value), }
				: GetAccentResource(accent.Color ?? WindowsTheme.Accent.Current);
		}

		private static ResourceDictionary GetAccentResource(Color color)
		{
			// Windows のテーマがアルファ チャネル 255 以外の色を返してくるけど、
			// HSV で Active と Highlight 用の色を作る過程で結局失われるので、
			// アルファ チャネルは 255 しかサポートしないようにしてしまおう感。
			color.A = 255;

			var hsv = color.ToHsv();
			var dark = hsv;
			var light = hsv;

			dark.V *= 0.8;
			light.S *= 0.6;

			var activeColor = dark.ToRgb();
			var highlightColor = light.ToRgb();

			var luminocity = Luminosity.FromRgb(color);
			var foreground = luminocity < 128 ? Colors.White : Colors.Black;

			var dic = new ResourceDictionary
			{
				["AccentColorKey"] = color,
				["AccentBrushKey"] = new SolidColorBrush(color),
				["AccentActiveColorKey"] = activeColor,
				["AccentActiveBrushKey"] = new SolidColorBrush(activeColor),
				["AccentHighlightColorKey"] = highlightColor,
				["AccentHighlightBrushKey"] = new SolidColorBrush(highlightColor),
				["AccentForegroundColorKey"] = foreground,
				["AccentForegroundBrushKey"] = new SolidColorBrush(foreground),
			};

			return dic;
		}

		private void UpdateListener(Accent accent)
		{
			if (accent == Accent.Windows)
			{
				if (this.windowsAccentListener == null)
				{
					// アクセントが Windows 依存で、リスナーが未登録だったら購読する
					this.windowsAccentListener = WindowsTheme.Accent.RegisterListener(x => this.ChangeAccentCore(x));
				}
			}
			else if (this.windowsAccentListener != null)
			{
				// アクセントが Windows 依存でないのにリスナーが登録されてたら解除する
				this.windowsAccentListener.Dispose();
				this.windowsAccentListener = null;
			}
		}

		private void UpdateListener(Theme theme)
		{
			if (theme == Theme.Windows)
			{
				if (this.windowsThemeListener == null)
				{
					this.windowsThemeListener = WindowsTheme.Theme.RegisterListener(x => this.ChangeThemeCore(x));
				}
			}
			else if (this.windowsThemeListener != null)
			{
				this.windowsThemeListener.Dispose();
				this.windowsThemeListener = null;
			}
		}

		/// <summary>
		/// 指定した <see cref="Uri"/> がテーマのリソースを指す URI かどうかをチェックします。
		/// </summary>
		/// <returns><paramref name="uri"/> がテーマのリソースを指す URI の場合は true、それ以外の場合は false。</returns>
		private static bool CheckThemeResourceUri(Uri uri)
		{
			return themeTemplate.Match(templateBaseUri, uri) != null;
		}

		/// <summary>
		/// 指定した <see cref="Uri"/> がアクセント カラーのリソースを指す URI かどうかをチェックします。
		/// </summary>
		/// <returns><paramref name="uri"/> がアクセント カラーのリソースを指す URI の場合は true、それ以外の場合は false。</returns>
		private static bool CheckAccentResourceUri(Uri uri)
		{
			return accentTemplate.Match(templateBaseUri, uri) != null;
		}

		private static Uri CreateThemeResourceUri(Theme.SpecifiedColor theme)
		{
			var param = new Dictionary<string, string>
			{
				{ "theme", theme.ToString() },
			};
			return themeTemplate.BindByName(templateBaseUri, param);
		}

		private static Uri CreateAccentResourceUri(Accent.SpecifiedColor accent)
		{
			var param = new Dictionary<string, string>
			{
				{ "accent", accent.ToString() },
			};
			return accentTemplate.BindByName(templateBaseUri, param);
		}

		private static IEnumerable<ResourceDictionary> EnumerateDictionaries(ResourceDictionary dictionary)
		{
			if (dictionary.MergedDictionaries.Count == 0)
			{
				yield break;
			}

			foreach (var mergedDictionary in dictionary.MergedDictionaries)
			{
				yield return mergedDictionary;

				foreach (var other in EnumerateDictionaries(mergedDictionary))
				{
					yield return other;
				}
			}
		}

		private void InvokeOnUIDispatcher(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			(this.dispatcher ?? Application.Current.Dispatcher).BeginInvoke(action, priority);
		}

		#region INotifyPropertyChanged 

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion
		

		[Obsolete("Register メソッドを使用してください。")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Initialize(Application app, Theme theme, Accent accent)
		{
			this.Register(app, theme, accent);
		}
	}
}
