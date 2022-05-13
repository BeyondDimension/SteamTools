using System.Reflection;

namespace System.Runtime.InteropServices
{
    public static class RuntimeInformation2
    {
        static readonly Type typeRuntimeInformation = Type.GetType("System.Runtime.InteropServices.RuntimeInformation");

        const Architecture EmptyArchitecture = (Architecture)int.MinValue;
        static Architecture _OSArchitecture = EmptyArchitecture;
        public static Architecture OSArchitecture
        {
            get
            {
                if (_OSArchitecture == EmptyArchitecture)
                {
                    try
                    {
                        if (typeRuntimeInformation != null)
                        {
                            _OSArchitecture = (Architecture)(int)typeRuntimeInformation.GetProperty(nameof(OSArchitecture), BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
                        }
                    }
                    catch
                    {

                    }
                    _OSArchitecture = Environment2.Is64BitOperatingSystem ? Architecture.X64 : Architecture.X86;
                }
                return _OSArchitecture;
            }
        }
    }

    public enum Architecture
    {
        X86 = 0,
        X64 = 1,
        Arm = 2,
        Arm64 = 3,
        Wasm = 4,
        S390x = 5,
        LoongArch64 = 6,
        Armv6 = 7,
    }
}
