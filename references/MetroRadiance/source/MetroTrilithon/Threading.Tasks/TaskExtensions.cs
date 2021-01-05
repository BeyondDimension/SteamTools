using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroTrilithon.Threading.Tasks
{
	public static class TaskExtensions
	{
		public static void Forget(
			this Task task,
			[CallerMemberName] string callerMemberName = "",
			[CallerFilePath] string callerFilePath = "",
			[CallerLineNumber] int callerLineNumber = 0)
		{
			task.ContinueWith(
				x => TaskLog.Raise(new TaskLog(callerMemberName, callerFilePath, callerLineNumber, x.Exception)),
				TaskContinuationOptions.OnlyOnFaulted);
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
}
