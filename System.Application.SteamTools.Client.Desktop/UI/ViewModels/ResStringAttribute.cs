namespace System.Application.UI.ViewModels
{
    [Obsolete("Languages")]
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ResStringAttribute : Attribute
    {
        public string? ResId { get; }

        public ResStringAttribute(string? resId = null)
        {
            ResId = resId;
        }
    }
}