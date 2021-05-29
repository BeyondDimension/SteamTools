using System;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Xamarin.Forms
{
    public interface IVisualElement_ChangeVisualStateInternal
    {
        void ChangeVisualStateInternal();
    }

    public static partial class VisualElementExtensions
    {
        static readonly Lazy<MethodInfo> mChangeVisualStateInternal = new(() =>
        {
            var method = typeof(VisualElement).GetMethod("ChangeVisualStateInternal",
                BindingFlags.NonPublic | BindingFlags.Instance);
            return method;
        });

        public static void ChangeVisualStateInternal(this VisualElement visualElement)
        {
            if (visualElement is IVisualElement_ChangeVisualStateInternal @internal)
            {
                @internal.ChangeVisualStateInternal();
            }
            else
            {
                _ = mChangeVisualStateInternal.Value.Invoke(visualElement, null);
            }
        }
    }
}