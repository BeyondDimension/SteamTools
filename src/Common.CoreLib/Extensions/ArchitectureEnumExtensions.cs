using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace System;

public static class ArchitectureEnumExtensions
{
    public static ArchitectureFlags Convert(this Architecture architecture, bool hasFlags) => architecture switch
    {
        Architecture.Arm => ArchitectureFlags.Arm,
        Architecture.X86 => ArchitectureFlags.X86,
        Architecture.Arm64 =>
        hasFlags ? (ArchitectureFlags.Arm64 | ArchitectureFlags.Arm) : ArchitectureFlags.Arm64,
        Architecture.X64 =>
        hasFlags ? (ArchitectureFlags.X64 | ArchitectureFlags.X86) : ArchitectureFlags.X64,
        _ => throw new ArgumentOutOfRangeException(nameof(architecture), architecture, null),
    };
}
