using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SteamTools.Models
{
	public static class ProductInfo
	{
		private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
		private static readonly Lazy<string> titleLazy = new Lazy<string>(() => ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title);
		private static readonly Lazy<string> descriptionLazy = new Lazy<string>(() => ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description);
		private static readonly Lazy<string> companyLazy = new Lazy<string>(() => ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute))).Company);
		private static readonly Lazy<string> productLazy = new Lazy<string>(() => ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute))).Product);
		private static readonly Lazy<string> copyrightLazy = new Lazy<string>(() => ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute))).Copyright);
		private static readonly Lazy<string> trademarkLazy = new Lazy<string>(() => ((AssemblyTrademarkAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTrademarkAttribute))).Trademark);
		private static readonly Lazy<string> versionLazy = new Lazy<string>(() => $"{Version.ToString(3)}{(IsBetaRelease ? " β" : "")}{(Version.Revision == 0 ? "" : " rev." + Version.Revision)}");
		private static readonly Lazy<IReadOnlyCollection<Library>> librariesLazy = new Lazy<IReadOnlyCollection<Library>>(() => new List<Library>
		{
			new Library("MetroRadiance", new Uri("https://github.com/Grabacr07/MetroRadiance")),
			new Library("MetroTrilithon", new Uri("https://github.com/Grabacr07/MetroTrilithon")),
			new Library("Livet", new Uri("https://github.com/runceel/Livet")),
			new Library("StatefulModel", new Uri("https://github.com/ugaya40/StatefulModel")),
			new Library("Hardcodet.NotifyIcon", new Uri("https://github.com/HavenDV/Hardcodet.NotifyIcon.Wpf.NetCore")),
			new Library("System.Reactive", new Uri("https://github.com/dotnet/reactive")),
			new Library("Titanium-Web-Proxy", new Uri("https://github.com/justcoding121/Titanium-Web-Proxy")),
			new Library("Ninject", new Uri("https://github.com/ninject/Ninject")),
			new Library("log4net", new Uri("https://github.com/apache/logging-log4net")),
			new Library("ArchiSteamFarm", new Uri("https://github.com/JustArchiNET/ArchiSteamFarm")),
			new Library("SteamAchievementManager", new Uri("https://github.com/gibbed/SteamAchievementManager")),
			new Library("HourBoostr", new Uri("https://github.com/Ezzpify/HourBoostr")),
			new Library("ArchiSteamFarm", new Uri("https://github.com/JustArchiNET/ArchiSteamFarm")),
			new Library("WinAuth", new Uri("https://github.com/winauth/winauth")),
			new Library("SteamDesktopAuthenticator", new Uri("https://github.com/Jessecar96/SteamDesktopAuthenticator")),
            new Library("Idle Master Extended", new Uri("https://github.com/JonasNilson/idle_master_extended")),
			new Library("Costura.Fody", new Uri("https://github.com/Fody/Costura")),
		});


		public static string Title => titleLazy.Value;

		public static string Description => descriptionLazy.Value;

		public static string Company => companyLazy.Value;

		public static string Product => productLazy.Value;

		public static string Copyright => copyrightLazy.Value;

		public static string Trademark => trademarkLazy.Value;

		public static Version Version => assembly.GetName().Version;

		public static string VersionString => versionLazy.Value;

		public static IReadOnlyCollection<Library> Libraries => librariesLazy.Value;


		// ReSharper disable ConvertPropertyToExpressionBody
		public static bool IsBetaRelease
		{
			get
			{
#if BETA
				return true;
#else
				return false;
#endif
			}
		}

		public static bool IsDebug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}
		// ReSharper restore ConvertPropertyToExpressionBody

		public class Library
		{
			public string Name { get; private set; }
			public Uri Url { get; private set; }

			public Library(string name, Uri url)
			{
				this.Name = name;
				this.Url = url;
			}
		}
	}
}
