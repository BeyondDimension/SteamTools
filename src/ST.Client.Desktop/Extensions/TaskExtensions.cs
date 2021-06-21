using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
	public static class TaskExtensions
	{
		public static Task Forget(
			this Task task,
			[CallerMemberName] string callerMemberName = "",
			[CallerFilePath] string callerFilePath = "",
			[CallerLineNumber] int callerLineNumber = 0)
		{
            task.ContinueWith(
                   x => TaskLog.Raise(new TaskLog(callerMemberName, callerFilePath, callerLineNumber, x.Exception)),
                   TaskContinuationOptions.OnlyOnFaulted).ConfigureAwait(false);
            return task;
		}

		public static void ForgetAndDispose(
			this Task task,
			[CallerMemberName] string callerMemberName = "",
			[CallerFilePath] string callerFilePath = "",
			[CallerLineNumber] int callerLineNumber = 0)
		{
			task.ContinueWith(
				x => TaskLog.Raise(new TaskLog(callerMemberName, callerFilePath, callerLineNumber, x.Exception)),
				TaskContinuationOptions.OnlyOnFaulted).ContinueWith(s=>s.Dispose()).ConfigureAwait(false);
		}

		public static Task WhenAll(this IEnumerable<Task> tasks)
		{
			return Task.WhenAll(tasks);
		}

		public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
		{
			return Task.WhenAll(tasks);
		}
	}

	public class TaskLog
	{
		public string CallerMemberName { get; }

		public string CallerFilePath { get; }

		public int CallerLineNumber { get; }

		public Exception Exception { get; }

		public TaskLog(string callerMemberName, string callerFilePath, int callerLineNumber, Exception exception)
		{
			CallerMemberName = callerMemberName;
			CallerFilePath = callerFilePath;
			CallerLineNumber = callerLineNumber;
			Exception = exception;
		}


		public static EventHandler<TaskLog> Occured = (sender, e) =>
		{
			const string format = @"Unhandled Exception occured from Task.Forget()
-----------
Caller file  : {1}
             : line {2}
Caller member: {0}
Exception: {3}

";
			System.Diagnostics.Debug.WriteLine(format, e.CallerMemberName, e.CallerFilePath, e.CallerLineNumber, e.Exception);
		};

		internal static void Raise(TaskLog log)
		{
			Occured?.Invoke(typeof(TaskLog), log);
		}
	}
}
