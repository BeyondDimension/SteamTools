// Copyright © 2015 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
// https://github.com/cefsharp/CefSharp/blob/v89.0.170/CefSharp/Callback/TaskSetCookieCallback.cs

using CefNet;
using CefSharp.Internals;
using System.Threading.Tasks;

namespace CefSharp
{
    /// <summary>
    /// Provides a callback implementation of <see cref="ISetCookieCallback"/>.
    /// </summary>
    public class TaskSetCookieCallback : CefSetCookieCallback
    {
        private readonly TaskCompletionSource<bool> taskCompletionSource;
        private bool onComplete; //Only ever accessed on the same CEF thread, so no need for thread safety

        /// <summary>
        /// Default constructor
        /// </summary>
        public TaskSetCookieCallback()
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
        }

        protected override void OnComplete(bool success)
        {
            onComplete = true;

            taskCompletionSource.TrySetResultAsync(success);
        }

        /// <summary>
        /// Task used to await this callback
        /// </summary>
        public Task<bool> Task
        {
            get { return taskCompletionSource.Task; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var task = taskCompletionSource.Task;

                //If onComplete is false then ISetCookieCallback.OnComplete was never called,
                //so we'll set the result to false. Calling TrySetResultAsync multiple times
                //can result in the issue outlined in https://github.com/cefsharp/CefSharp/pull/2349
                if (onComplete == false && task.IsCompleted == false)
                {
                    taskCompletionSource.TrySetResultAsync(false);
                }
            }
            base.Dispose(disposing);
        }
    }
}