using System.Runtime.CompilerServices;
using static System.Properties.ThisAssembly;

[assembly: InternalsVisibleTo("System.Common.UnitTest" + PublicKey)]
[assembly: InternalsVisibleTo("System.Common.UnitTest.iOS" + PublicKey)]
[assembly: InternalsVisibleTo("System.Common.UnitTest.Droid" + PublicKey)]
[assembly: InternalsVisibleTo("System.Application.SteamTools.Client.UnitTest" + PublicKey)]
[assembly: InternalsVisibleTo("System.Application.SteamTools.Client.Desktop.UnitTest" + PublicKey)]
[assembly: InternalsVisibleTo("System.Application.SteamTools.Client.Droid.UnitTest" + PublicKey)]
[assembly: InternalsVisibleTo("System.Application.SteamTools.Client.Droid.UnitTest.App" + PublicKey)]