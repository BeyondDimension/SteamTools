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
