using System.Reflection;
using _ThisAssembly = System.Properties.ThisAssembly;

[assembly: AssemblyFileVersion(_ThisAssembly.Version)]
//[assembly: AssemblyInformationalVersion(_ThisAssembly.InfoVersion)]
[assembly: AssemblyVersion(_ThisAssembly.Version)]