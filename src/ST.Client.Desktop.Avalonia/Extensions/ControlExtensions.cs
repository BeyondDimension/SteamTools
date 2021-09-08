using Avalonia.Controls;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class ControlExtensions
    {
        public static IControl? FindParentControl(this IControl? c, string name)
        {
            if (c != null)
            {
                if (c.Name == name)
                {
                    return c;
                }
                if (c.Parent != null)
                {
                    return c.Parent.FindParentControl(name);
                }
            }
            return null;
        }

        public static T? FindParentControl<T>(this IControl? c, string name) where T : class, IControl
        {
            if (c != null)
            {
                if (c.Name == name && c is T t)
                {
                    return t;
                }
                if (c.Parent != null)
                {
                    return c.Parent.FindParentControl<T>(name);
                }
            }
            return null;
        }
    }
}