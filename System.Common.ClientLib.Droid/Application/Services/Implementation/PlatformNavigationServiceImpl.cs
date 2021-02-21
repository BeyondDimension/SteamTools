using Android.Content;
using Activity = Android.App.Activity;
using XEPlatform = Xamarin.Essentials.Platform;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="INavigationService"/>
    public abstract class PlatformNavigationServiceImpl : IPlatformNavigationService
    {
        protected virtual string TypeNameTemplate { get; } = "System.Application.UI.Activities.{0}Activity";

        protected virtual string GetRouteName(Type uiControllerType)
        {
            var routeName = uiControllerType.Name.TrimEnd("Controller");
            if (uiControllerType.IsInterface) routeName = routeName.TrimStart('I');
            return routeName;
        }

        public Type GetActivityType(Type uiControllerType)
        {
            if (uiControllerType.IsSubclassOf(typeof(Activity)))
            {
                return uiControllerType;
            }
            var routeName = GetRouteName(uiControllerType);
            var typeName = string.Format(TypeNameTemplate, routeName);
            uiControllerType = Type.GetType(typeName, throwOnError: true);
            return uiControllerType;
        }

        public void Push(Type type, PushFlags flags = PushFlags.Empty)
        {
            type = GetActivityType(type);
            var currentActivity = XEPlatform.CurrentActivity;
            if (flags == PushFlags.Empty)
            {
                currentActivity.StartActivity(type);
            }
            else
            {
                var intent = new Intent(currentActivity, type);

                // ActivityFlags
                if (flags.HasFlag(PushFlags.ClearTop))
                    intent.AddFlags(ActivityFlags.ClearTop);
                if (flags.HasFlag(PushFlags.SingleTop))
                    intent.AddFlags(ActivityFlags.SingleTop);

                currentActivity.StartActivity(intent);

                // After StartActivity Flags
                if (flags.HasFlag(PushFlags.OverridePendingTransitionZeroZero))
                    currentActivity.OverridePendingTransition(0, 0);
                if (flags.HasFlag(PushFlags.Finish))
                    currentActivity.Finish();
            }
        }

        public void Pop() => XEPlatform.CurrentActivity?.OnBackPressed();
    }
}