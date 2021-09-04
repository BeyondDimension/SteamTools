//using Android.App;
//using Android.Appwidget;
//using Android.Content;
//using Android.Widget;

//// https://developer.android.google.cn/guide/topics/appwidgets?hl=zh-cn#java

//namespace System.Application.Receivers
//{
//    /// <summary>
//    /// 本地令牌 - 应用微件
//    /// <para>应用微件是可以嵌入其他应用（如主屏幕）并接收定期更新的微型应用视图</para>
//    /// </summary>
//    [BroadcastReceiver]
//    [IntentFilter(new[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
//    [MetaData("android.appwidget.provider", Resource = "@xml/local_auth_appwidget_info")]
//    internal sealed class LocalAuthAppWidgetProvider : AppWidgetProvider
//    {
//        public override void OnUpdate(Context? context,
//            AppWidgetManager? appWidgetManager, int[]? appWidgetIds)
//        {
//            if (context == null || appWidgetManager == null) return;
//            var componentName = new ComponentName(context, typeof(LocalAuthAppWidgetProvider).GetJClass().Name);
//            appWidgetManager.UpdateAppWidget(componentName, BuildRemoteViews(context, appWidgetIds));
//        }

//        RemoteViews BuildRemoteViews(Context context, int[]? appWidgetIds)
//        {
//            // Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
//            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.local_auth_appwidget);

//            //SetTextViewText(widgetView);
//            //RegisterClicks(context, appWidgetIds, widgetView);

//            return widgetView;
//        }
//    }
//}