using Android.App;

// 全面屏兼容 max_aspect
[assembly: MetaData("android.max_aspect", Value = "2.4")]
// 允许多窗口运行
[assembly: MetaData("android.allow_multiple_resumed_activities", Value = "true")]