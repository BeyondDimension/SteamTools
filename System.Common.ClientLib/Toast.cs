using System.Application.Services;

namespace System
{
    /// <inheritdoc cref="IToast"/>
    public static class Toast
    {
        /// <inheritdoc cref="IToast.Show(string, int?)"/>
        public static void Show(string text, int? duration = null)
        {
            var toast = DI.Get<IToast>();
            toast.Show(text, duration);
        }

        /// <inheritdoc cref="IToast.Show(string, ToastLength)"/>
        public static void Show(string text, ToastLength duration)
        {
            var toast = DI.Get<IToast>();
            toast.Show(text, duration);
        }
    }
}