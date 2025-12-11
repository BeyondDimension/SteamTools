// ReSharper disable once CheckNamespace
namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
[Conditional("JETBRAINS_ANNOTATIONS")]
sealed class PublicAPIAttribute : Attribute
{
    public string? Comment { get; }

    public PublicAPIAttribute()
    {
    }

    public PublicAPIAttribute(string comment)
    {
        Comment = comment;
    }
}