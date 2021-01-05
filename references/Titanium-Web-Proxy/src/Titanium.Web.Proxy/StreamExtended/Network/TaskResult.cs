using System;
using System.Threading;
using System.Threading.Tasks;

namespace Titanium.Web.Proxy.StreamExtended.Network
{
    /// <summary>
    /// Mimic a Task but you can set AsyncState
    /// </summary>
    public class TaskResult : IAsyncResult
    {
        Task Task;
        readonly object asyncState;

        public TaskResult(Task pTask, object state)
        {
            Task = pTask;
            asyncState = state;
        }

        public object AsyncState => asyncState;

        public WaitHandle AsyncWaitHandle => ((IAsyncResult)Task).AsyncWaitHandle;

        public bool CompletedSynchronously => ((IAsyncResult)Task).CompletedSynchronously;

        public bool IsCompleted => Task.IsCompleted;

        public void GetResult() { this.Task.GetAwaiter().GetResult(); }
    }

    /// <summary>
    /// Mimic a Task&lt;T&gt; but you can set AsyncState
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TaskResult<T> : IAsyncResult
    {
        Task<T> Task;
        readonly object asyncState;

        public TaskResult(Task<T> pTask, object state)
        {
            Task = pTask;
            asyncState = state;
        }

        public object AsyncState => asyncState;

        public WaitHandle AsyncWaitHandle => ((IAsyncResult)Task).AsyncWaitHandle;

        public bool CompletedSynchronously => ((IAsyncResult)Task).CompletedSynchronously;

        public bool IsCompleted => Task.IsCompleted;

        public T Result => Task.Result;
    }
}
