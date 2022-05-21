// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// https://github.com/PowerShell/PowerShell/blob/v7.2.4/src/System.Management.Automation/utils/ArchitectureSensitiveAttribute.cs

namespace System.Management.Automation.Internal
{
    /// <summary>
    /// This attribute is used for Design For Testability.
    /// It should be placed on any method containing code
    /// which is likely to be sensitive to X86/X64/IA64 issues,
    /// primarily code which calls DllImports or otherwise uses
    /// NativeMethods.  This allows us to generate code coverage
    /// data specific to architecture sensitive code.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal class ArchitectureSensitiveAttribute : Attribute
    {
        /// <summary>
        /// Constructor for the ArchitectureSensitiveAttribute class.
        /// </summary>
        internal ArchitectureSensitiveAttribute()
        {
        }
    }
}