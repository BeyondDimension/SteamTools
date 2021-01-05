using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MetroRadiance.Interop.Win32;
using MetroRadiance.Media;
using MetroRadiance.Platform;
using MetroRadiance.Utilities;

#if NETCOREAPP
using Tavis.UriTemplates;
#endif

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

		private static readonly string _baseUrl = @"pack://application:,,,/MetroRadiance;component/";
		private static readonly string _themeUrl = @"Themes/{theme}.xaml";
		private static readonly string _uwpThemeUrl = @"Themes/UWP/{theme}.xaml";
		private static readonly string _accentUrl = @"Themes/Accents/{accent}.xaml";
		private static readonly string _uwpAccentUrl = @"Themes/UWP/Accents/{accent}.xaml";
		private static readonly UriTemplateTable _templateTable;

		private static readonly UriTemplate _themeTemplate = new UriTemplate(_themeUrl);
		private static readonly UriTemplate _uwpThemeTemplate = new UriTemplate(_uwpThemeUrl);
		private static readonly UriTemplate _accentTemplate = new UriTemplate(_accentUrl);
		private static readonly UriTemplate _uwpAccentTemplate = new UriTemplate(_uwpAccentUrl);
		private static readonly Uri _templateBaseUri = new Uri(_baseUrl);

		private bool _enableUWPCompatibleResources;
		private Dispatcher _dispatcher;
		private IDisposable _windowsAccentListener;
		private IDisposable _windowsThemeListener;

		private readonly List<ResourceDictionary> _themeResources = new List<ResourceDictionary>();
		private readonly List<ResourceDictionary> _uwpThemeResources = new List<ResourceDictionary>();
		private readonly List<ResourceDictionary> _accentResources = new List<ResourceDictionary>();
		private readonly List<ResourceDictionary> _uwpAccentResources = new List<ResourceDictionary>();

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

		static ThemeService()
		{
#if NETCOREAPP
			_templateTable = new UriTemplateTable();
			_templateTable.Add("theme", new UriTemplate(_baseUrl + _themeUrl));
			_templateTable.Add("accent", new UriTemplate(_baseUrl + _accentUrl));
			_templateTable.Add("uwptheme", new UriTemplate(_baseUrl + _uwpThemeUrl));
			_templateTable.Add("uwpaccent", new UriTemplate(_baseUrl + _uwpAccentUrl));
#else
			_templateTable = new UriTemplateTable(_templateBaseUri);
			_templateTable.KeyValuePairs.Add(new KeyValuePair<UriTemplate, Object>(_themeTemplate, "theme"));
			_templateTable.KeyValuePairs.Add(new KeyValuePair<UriTemplate, Object>(_accentTemplate, "accent"));
			_templateTable.KeyValuePairs.Add(new KeyValuePair<UriTemplate, Object>(_uwpThemeTemplate, "uwptheme"));
			_templateTable.KeyValuePairs.Add(new KeyValuePair<UriTemplate, Object>(_uwpAccentTemplate, "uwpaccent"));
			_templateTable.MakeReadOnly(false);
#endif
		}

		private ThemeService() { }

		/// <summary>
		/// Enable UWP resources.
		///   ColorKey: System[Simple Light/Dark Name]Color
		///   HighContrastColorKey: System[Simple HighContrast Name]Color
		///   BrushKey: SystemControl[Simple HighContrast name][Simple light/dark name]Brush
		/// EnableUWPResoruces() need to be called befer calling Register().
		/// 
		/// Refferences:
		///  - https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/xaml-theme-resources#the-xaml-color-ramp-and-theme-dependent-brushes
		///  - https://docs.microsoft.com/en-us/windows/uwp/design/style/color#accent-color-palette
		/// </summary>
		public void EnableUwpResoruces()
		{
			Debug.Assert(!this._themeResources.Any() && !this._uwpThemeResources.Any()
				&& !this._accentResources.Any() && !this._uwpAccentResources.Any(),
				"Warning: ThemeService.EnableUWPResoruces() need to be called befer calling ThemeService.Register()");
			this._enableUWPCompatibleResources = true;
		}

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
			this._dispatcher = app.Dispatcher;

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

			// MetroRadiance Theme
			var themeDic = GetThemeResource(theme);
			var targetThemeDic = allDictionaries.LastOrDefault(x => CheckThemeResourceUri(x.Source));
			targetThemeDic = UpdateDic(rd, targetThemeDic, themeDic);
			this._themeResources.Add(targetThemeDic);

			// UWP Theme
			ResourceDictionary targetUwpThemeDic = null;
			var upwThemeDic = GetUwpThemeResource(theme);
			targetUwpThemeDic = allDictionaries.LastOrDefault(x => CheckUwpThemeResourceUri(x.Source));
			if (targetUwpThemeDic != null || this._enableUWPCompatibleResources)
			{
				targetUwpThemeDic = UpdateDic(rd, targetUwpThemeDic, upwThemeDic);
				this._uwpThemeResources.Add(targetUwpThemeDic);
			}

			// MetroRadiance Accent
			var accentDic = GetAccentResource(accent);
			var targetAccentDic = allDictionaries.LastOrDefault(x => CheckAccentResourceUri(x.Source));
			targetAccentDic = UpdateDic(rd, targetAccentDic, accentDic);
			this._accentResources.Add(targetAccentDic);

			// UWP Accent
			var uwpAccentDic = GetUwpAccentResource(accent);
			if (targetUwpThemeDic != null)
			{
				// UWP Themeにより、追加されている可能性があるので、再取得する
				allDictionaries = EnumerateDictionaries(rd).ToArray();
			}
			var targetUwpAccentDic = allDictionaries.LastOrDefault(x => CheckUwpAccentResourceUri(x.Source));
			if (targetUwpAccentDic != null || this._enableUWPCompatibleResources)
			{
				targetUwpAccentDic = UpdateDic(rd, targetUwpAccentDic, uwpAccentDic);
				this._uwpAccentResources.Add(targetUwpAccentDic);
			}

			// Unregister したいときは戻り値の IDisposable を Dispose() してほしい
			return Disposable.Create(() =>
			{
				this._themeResources.Remove(targetThemeDic);
				if (targetUwpThemeDic != null)
				{
					this._uwpThemeResources.Remove(targetUwpThemeDic);
				}
				this._accentResources.Remove(targetAccentDic);
				if (targetUwpAccentDic != null)
				{
					this._uwpAccentResources.Remove(targetUwpAccentDic);
				}
			});
		}

		ResourceDictionary UpdateDic(ResourceDictionary registertDic, ResourceDictionary targetDic, ResourceDictionary baseDic)
		{
			if (targetDic == null)
			{
				targetDic = baseDic;
				registertDic.MergedDictionaries.Add(targetDic);
			}
			else
			{
				foreach (var key in baseDic.Keys.OfType<string>().Where(x => targetDic.Contains(x)))
				{
					targetDic[key] = baseDic[key];
				}
			}
			return targetDic;
		}

		public void ChangeTheme(Theme theme)
		{
			if (this.Theme == theme) return;

			this.InvokeOnUIDispatcher(() => this.ChangeThemeCore(theme));
			this.Theme = theme;
		}

		void ChangeThemeCore(Platform.Theme theme)
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

		void ChangeThemeCore(Theme theme)
		{
			this.SetAppMode(theme);

			var dic = GetThemeResource(theme);

			foreach (var key in dic.Keys.OfType<string>())
			{
				foreach (var resource in this._themeResources.Where(x => x.Contains(key)))
				{
					resource[key] = dic[key];
				}
			}

			var uwpDic = GetUwpThemeResource(theme);

			foreach (var key in uwpDic.Keys.OfType<string>())
			{
				foreach (var resource in this._uwpThemeResources.Where(x => x.Contains(key)))
				{
					resource[key] = uwpDic[key];
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

		void ChangeAccentCore(Accent accent)
		{
			this.ChangeAccentCore(GetAccentResource(accent), GetUwpAccentResource(accent));
		}

		void ChangeAccentCore(Color color)
		{
			this.ChangeAccentCore(GetAccentResource(color), GetUwpAccentResource(color));
		}

		void ChangeAccentCore(ResourceDictionary dic, ResourceDictionary uwpDic)
		{
			foreach (var key in dic.Keys.OfType<string>())
			{
				foreach (var resource in this._accentResources.Where(x => x.Contains(key)))
				{
					resource[key] = dic[key];
				}
			}
			foreach (var key in uwpDic.Keys.OfType<string>())
			{
				foreach (var resource in this._uwpAccentResources.Where(x => x.Contains(key)))
				{
					resource[key] = uwpDic[key];
				}
			}
		}

		static ResourceDictionary GetThemeResource(Theme theme)
		{
			var specified = theme.SyncToWindows
				? WindowsTheme.Theme.Current == Platform.Theme.Dark ? Theme.Dark.Specified : Theme.Light.Specified
				: theme.Specified;
			if (specified == null) throw new ArgumentException($"Invalid theme value '{theme}'.");

			return new ResourceDictionary { Source = CreateThemeResourceUri(specified.Value), };
		}

		static ResourceDictionary GetUwpThemeResource(Theme theme)
		{
			var specified = theme.SyncToWindows
				? WindowsTheme.Theme.Current == Platform.Theme.Dark ? Theme.Dark.Specified : Theme.Light.Specified
				: theme.Specified;
			if (specified == null) throw new ArgumentException($"Invalid theme value '{theme}'.");

			return new ResourceDictionary { Source = CreateUwpThemeResourceUri(specified.Value), };
		}

		static ResourceDictionary GetAccentResource(Accent accent)
		{
			return accent.Specified != null
				? new ResourceDictionary { Source = CreateAccentResourceUri(accent.Specified.Value), }
				: GetAccentResource(accent.Color ?? WindowsTheme.Accent.Current);
		}
		static ResourceDictionary GetUwpAccentResource(Accent accent)
		{
			return accent.Specified != null
				? new ResourceDictionary { Source = CreateUwpAccentResourceUri(accent.Specified.Value), }
				: GetUwpAccentResource(accent.Color ?? WindowsTheme.Accent.Current);
		}

		static ResourceDictionary GetAccentResource(Color color)
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

			var dic = new ResourceDictionary()
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

		static ResourceDictionary GetUwpAccentResource(Color color)
		{
			// Windows のテーマがアルファ チャネル 255 以外の色を返してくるけど、
			// アルファ チャネルは 255 しかサポートしないようにしてしまおう感。
			color.A = 255;
			var hsl = color.ToHsl();
			var dic = new ResourceDictionary()
			{
				["SystemAccentColorLigth3"] = HslColor.FromHsl(hsl.H, hsl.S, hsl.L + 0.175).ToRgb(),
				["SystemAccentColorLigth2"] = HslColor.FromHsl(hsl.H, hsl.S, hsl.L + 0.1025).ToRgb(),
				["SystemAccentColorLigth1"] = HslColor.FromHsl(hsl.H, hsl.S, hsl.L + 0.05).ToRgb(),
				["SystemAccentColor"] = color,
				["SystemAccentColorDark1"] = HslColor.FromHsl(hsl.H, hsl.S, hsl.L - 0.05).ToRgb(),
				["SystemAccentColorDark2"] = HslColor.FromHsl(hsl.H, hsl.S, hsl.L - 0.1025).ToRgb(),
				["SystemAccentColorDark3"] = HslColor.FromHsl(hsl.H, hsl.S, hsl.L - 0.175).ToRgb(),
			};
			return dic;
		}

		void UpdateListener(Accent accent)
		{
			if (accent == Accent.Windows)
			{
				if (this._windowsAccentListener == null)
				{
					// アクセントが Windows 依存で、リスナーが未登録だったら購読する
					this._windowsAccentListener = WindowsTheme.Accent.RegisterListener(x => this.ChangeAccentCore(x));
				}
			}
			else if (this._windowsAccentListener != null)
			{
				// アクセントが Windows 依存でないのにリスナーが登録されてたら解除する
				this._windowsAccentListener.Dispose();
				this._windowsAccentListener = null;
			}
		}

		void UpdateListener(Theme theme)
		{
			if (theme == Theme.Windows)
			{
				if (this._windowsThemeListener == null)
				{
					this._windowsThemeListener = WindowsTheme.Theme.RegisterListener(x => this.ChangeThemeCore(x));
				}
			}
			else if (this._windowsThemeListener != null)
			{
				this._windowsThemeListener.Dispose();
				this._windowsThemeListener = null;
			}
		}

		/// <summary>
		/// 指定した <see cref="Uri"/> がテーマのリソースを指す URI かどうかをチェックします。
		/// </summary>
		/// <returns><paramref name="uri"/> がテーマのリソースを指す URI の場合は true、それ以外の場合は false。</returns>
		static bool CheckThemeResourceUri(Uri uri)
		{
			if (uri == null) return false;
#if NETCOREAPP
			return _templateTable.Match(uri)?.Key == "theme";
#else
#if false
			var result = _templateTable.Match(uri);
			return result != null && result.Count == 1 && result.First().Data.ToString() == "theme";
#else
			return _themeTemplate.Match(_templateBaseUri, uri) != null;
#endif
#endif
		}

		/// <summary>
		/// 指定した <see cref="Uri"/> がUWPテーマのリソースを指す URI かどうかをチェックします。
		/// </summary>
		/// <returns><paramref name="uri"/> UWPがテーマのリソースを指す URI の場合は true、それ以外の場合は false。</returns>
		static bool CheckUwpThemeResourceUri(Uri uri)
		{
			if (uri == null) return false;
#if NETCOREAPP
			return _templateTable.Match(uri)?.Key == "uwptheme";
#else
#if false
			var result = _templateTable.Match(uri);
			return result != null && result.Count == 1 && result.First().Data.ToString() == "uwptheme";
#else
			return _uwpThemeTemplate.Match(_templateBaseUri, uri) != null;
#endif
#endif
		}

		/// <summary>
		/// 指定した <see cref="Uri"/> がアクセント カラーのリソースを指す URI かどうかをチェックします。
		/// </summary>
		/// <returns><paramref name="uri"/> がアクセント カラーのリソースを指す URI の場合は true、それ以外の場合は false。</returns>
		static bool CheckAccentResourceUri(Uri uri)
		{
			if (uri == null) return false;
#if NETCOREAPP
			return _templateTable.Match(uri)?.Key == "accent";
#else
#if false
			var result = _templateTable.Match(uri);
			return result != null && result.Count == 1 && result.First().Data.ToString() == "accent";
#else
			return _accentTemplate.Match(_templateBaseUri, uri) != null;
#endif
#endif
		}


		/// <summary>
		/// 指定した <see cref="Uri"/> がUWPアクセント カラーのリソースを指す URI かどうかをチェックします。
		/// </summary>
		/// <returns><paramref name="uri"/> がUWPアクセント カラーのリソースを指す URI の場合は true、それ以外の場合は false。</returns>
		static bool CheckUwpAccentResourceUri(Uri uri)
		{
			if (uri == null) return false;
#if NETCOREAPP
			return _templateTable.Match(uri)?.Key == "uwpaccent";
#else
#if false
			var result = _templateTable.Match(uri);
			return result != null && result.Count == 1 && result.First().Data.ToString() == "uwpaccent";
#else
			return _uwpAccentTemplate.Match(_templateBaseUri, uri) != null;
#endif
#endif
		}

		static Uri CreateThemeResourceUri(Theme.SpecifiedColor theme)
		{
#if NETCOREAPP
			var url = _themeTemplate
				.AddParameters(new
				{
					theme = theme.ToString()
				})
				.Resolve();
			return new Uri(_templateBaseUri, url);
#else
			var param = new Dictionary<string, string>
			{
				{ "theme", theme.ToString() },
			};
			return _themeTemplate.BindByName(_templateBaseUri, param);
#endif
		}

		static Uri CreateUwpThemeResourceUri(Theme.SpecifiedColor theme)
		{
#if NETCOREAPP
			var url = _uwpThemeTemplate
				.AddParameters(new
				{
					theme = theme.ToString()
				})
				.Resolve();
			return new Uri(_templateBaseUri, url);
#else
			var param = new Dictionary<string, string>
			{
				{ "theme", theme.ToString() },
			};
			return _uwpThemeTemplate.BindByName(_templateBaseUri, param);
#endif
		}

		static Uri CreateAccentResourceUri(Accent.SpecifiedColor accent)
		{
#if NETCOREAPP
			var url = _accentTemplate
				.AddParameters(new
				{
					accent = accent.ToString()
				})
				.Resolve();
			return new Uri(_templateBaseUri, url);
#else
			var param = new Dictionary<string, string>
			{
				{ "accent", accent.ToString() },
			};
			return _accentTemplate.BindByName(_templateBaseUri, param);
#endif
		}

		static Uri CreateUwpAccentResourceUri(Accent.SpecifiedColor accent)
		{
#if NETCOREAPP
			var url = _uwpAccentTemplate
				.AddParameters(new
				{
					accent = accent.ToString()
				})
				.Resolve();
			return new Uri(_templateBaseUri, url);
#else
			var param = new Dictionary<string, string>
			{
				{ "accent", accent.ToString() },
			};
			return _uwpAccentTemplate.BindByName(_templateBaseUri, param);
#endif
		}

		static IEnumerable<ResourceDictionary> EnumerateDictionaries(ResourceDictionary dictionary)
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

		void InvokeOnUIDispatcher(Action action, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			(this._dispatcher ?? Application.Current.Dispatcher).BeginInvoke(action, priority);
		}

#region INotifyPropertyChanged 

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

#endregion


		[Obsolete("Use Register() method")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void Initialize(Application app, Theme theme, Accent accent)
		{
			this.Register(app, theme, accent);
		}
	}
}
