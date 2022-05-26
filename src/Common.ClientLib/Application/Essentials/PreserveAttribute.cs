using System.ComponentModel;

// https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/Types/Shared/PreserveAttribute.shared.cs

// ReSharper disable once CheckNamespace
namespace Xamarin.Essentials;

[AttributeUsage(AttributeTargets.All)]
[EditorBrowsable(EditorBrowsableState.Never)]
sealed class PreserveAttribute : Attribute
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
