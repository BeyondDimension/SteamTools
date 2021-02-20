using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// <see cref="Fragment"/> 扩展
    /// </summary>
    public static partial class FragmentExtensions
    {
        public static bool HasValue(this Fragment? fragment)
        {
            return fragment != null && !fragment.IsDetached && fragment.Activity.HasValue();
        }

#if DEBUG

        [Obsolete("use HasValue", true)]
        public static bool IsAvailable(this Fragment? fragment) => fragment.HasValue();

#endif
    }
}