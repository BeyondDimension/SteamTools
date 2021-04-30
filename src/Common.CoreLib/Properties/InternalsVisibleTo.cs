using System.Runtime.CompilerServices;
using static System.Properties.ThisAssembly;

[assembly: InternalsVisibleTo("System.Common.UnitTest" + PublicKey)]
[assembly: InternalsVisibleTo("System.Common.UnitTest.iOS" + PublicKey)]
[assembly: InternalsVisibleTo("System.Common.UnitTest.Droid" + PublicKey)]