// https://github.com/xamarin/Essentials/blob/1.7.3/Xamarin.Essentials/Types/Shared/PreserveAttribute.shared.cs
using System.ComponentModel;

#if NETSTANDARD
namespace System.Diagnostics.CodeAnalysis;

[AttributeUsage(AttributeTargets.All)]
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class PreserveAttribute : Attribute
{
#pragma warning disable SA1401 // Fields should be private
    public bool AllMembers;
    public bool Conditional;
#pragma warning restore SA1401 // Fields should be private

    public PreserveAttribute(bool allMembers, bool conditional)
    {
        AllMembers = allMembers;
        Conditional = conditional;
    }

    public PreserveAttribute()
    {
    }
}
#endif