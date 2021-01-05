using System;

namespace MetroTrilithon.Threading.Tasks
{
	public class TaskLog
	{
		public string CallerMemberName { get; }

		public string CallerFilePath { get; }

		public int CallerLineNumber { get; }

		public Exception Exception { get; }

		public TaskLog(string callerMemberName, string callerFilePath, int callerLineNumber, Exception exception)
		{
			this.CallerMemberName = callerMemberName;
			this.CallerFilePath = callerFilePath;
			this.CallerLineNumber = callerLineNumber;
			this.Exception = exception;
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
