using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AndroidAppActivity = Android.App.Activity;
#if NET6_0_OR_GREATER
using XEPlatform = Microsoft.Maui.ApplicationModel.Platform;
#else
using XEPlatform = Xamarin.Essentials.Platform;
#endif
using AndroidAppResult = Android.App.Result;

// 将 onActivityResult() 转换为 System.Threading.Tasks.Task
// 参考 https://github.com/xamarin/Essentials/blob/1.7.0/Xamarin.Essentials/Platform/Platform.android.cs

// ReSharper disable once CheckNamespace
namespace AndroidX.Activity.Result
{
    internal static class ActivityResultTask
    {
        internal const int requestCodeSaveFileDialog = 110010;
        internal const int requestCodeVpnService = 110011;

        internal const int requestCodeStart = 120000;

        static int requestCode = requestCodeStart;

        internal static int NextRequestCode()
        {
            if (++requestCode >= 12999)
                requestCode = requestCodeStart;

            return requestCode;
        }

        [Activity(ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
        internal class IntermediateActivity : AndroidAppActivity
        {
            const string launchedExtra = "launched";
            const string actualIntentExtra = "actual_intent";
            const string guidExtra = "guid";
            const string requestCodeExtra = "request_code";
            const string resultExtra = "droid_app_result";

            static readonly ConcurrentDictionary<string?, IntermediateTask> pendingTasks =
                new();

            bool launched;
            Intent? actualIntent;
            string? guid;
            int requestCode;

            protected override void OnCreate(Bundle? savedInstanceState)
            {
                base.OnCreate(savedInstanceState);

                var extras = savedInstanceState ?? Intent!.Extras;

                // read the values
                launched = extras!.GetBoolean(launchedExtra, false);
                actualIntent = extras.GetParcelable(actualIntentExtra) as Intent;
                guid = extras.GetString(guidExtra);
                requestCode = extras.GetInt(requestCodeExtra, -1);

                if (GetIntermediateTask(guid) is IntermediateTask task)
                {
                    task.OnCreate?.Invoke(actualIntent);
                }

                // if this is the first time, lauch the real activity
                if (!launched)
                    StartActivityForResult(actualIntent, requestCode);
            }

            protected override void OnSaveInstanceState(Bundle outState)
            {
                // make sure we mark this activity as launched
                outState.PutBoolean(launchedExtra, true);

                // save the values
                outState.PutParcelable(actualIntentExtra, actualIntent);
                outState.PutString(guidExtra, guid);
                outState.PutInt(requestCodeExtra, requestCode);

                base.OnSaveInstanceState(outState);
            }

            protected override void OnActivityResult(int requestCode, AndroidAppResult resultCode, Intent? data)
            {
                base.OnActivityResult(requestCode, resultCode, data);

                // we have a valid GUID, so handle the task
                if (GetIntermediateTask(guid, true) is IntermediateTask task)
                {
                    if (resultCode == AndroidAppResult.Canceled)
                    {
                        task.TaskCompletionSource.TrySetCanceled();
                    }
                    else
                    {
                        try
                        {
                            data ??= new Intent();

                            task.OnResult?.Invoke(data);

                            task.TaskCompletionSource.TrySetResult(data);
                        }
                        catch (Exception ex)
                        {
                            task.TaskCompletionSource.TrySetException(ex);
                        }
                    }
                }

                // close the intermediate activity
                Finish();
            }

            public static Task<Intent> StartAsync(Intent intent, int requestCode, Action<Intent?>? onCreate = null, Action<Intent>? onResult = null)
            {
                // make sure we have the activity
                var activity = XEPlatform.CurrentActivity;
                if (activity == null)
                    throw new NullReferenceException("The current Activity can not be detected. Ensure that you have called Init in your Activity or Application class.");

                // create a new task
                var data = new IntermediateTask(onCreate, onResult);
                pendingTasks[data.Id] = data;

                // create the intermediate intent, and add the real intent to it
                var intermediateIntent = new Intent(activity, typeof(IntermediateActivity));
                intermediateIntent.PutExtra(actualIntentExtra, intent);
                intermediateIntent.PutExtra(guidExtra, data.Id);
                intermediateIntent.PutExtra(requestCodeExtra, requestCode);

                // start the intermediate activity
                activity.StartActivityForResult(intermediateIntent, requestCode);

                return data.TaskCompletionSource.Task;
            }

            static IntermediateTask? GetIntermediateTask(string? guid, bool remove = false)
            {
                if (string.IsNullOrEmpty(guid))
                    return null;

                if (remove)
                {
                    pendingTasks.TryRemove(guid, out var removedTask);
                    return removedTask;
                }

                pendingTasks.TryGetValue(guid, out var task);
                return task;
            }

            class IntermediateTask
            {
                public IntermediateTask(Action<Intent?>? onCreate, Action<Intent>? onResult)
                {
                    Id = Guid.NewGuid().ToString();
                    TaskCompletionSource = new TaskCompletionSource<Intent>();

                    OnCreate = onCreate;
                    OnResult = onResult;
                }

                public string Id { get; }

                public TaskCompletionSource<Intent> TaskCompletionSource { get; }

                public Action<Intent?>? OnCreate { get; }

                public Action<Intent>? OnResult { get; }
            }

            public static Intent SetResult(Intent? data, AndroidAppResult resultCode)
            {
                if (data == null) data = new();
                data.PutExtra(resultExtra, (int)resultCode);
                return data;
            }

            public static AndroidAppResult GetResult(Intent data, int defaultValue = int.MinValue)
                => (AndroidAppResult)data.GetIntExtra(resultExtra, defaultValue);
        }
    }
}
