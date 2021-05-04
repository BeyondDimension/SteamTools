// Copyright © 2014 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
// https://github.com/cefsharp/CefSharp/blob/v89.0.170/CefSharp/Internals/TaskExtensions.cs

using System.Threading;
using System.Threading.Tasks;

namespace CefSharp.Internals
{
    /// <summary>
    /// TaskExtension based on the following
    /// https://github.com/ChadBurggraf/parallel-extensions-extras/blob/master/Extensions/TaskExtrasExtensions.cs
    /// https://github.com/ChadBurggraf/parallel-extensions-extras/blob/ec803e58eee28c698e44f55f49c5ad6671b1aa58/Extensions/TaskCompletionSourceExtensions.cs
    /// </summary>
    internal static class TaskExtensions
    {
        /// <summary>
        /// Set the TaskCompletionSource in an async fashion. This prevents the Task Continuation being executed sync on the same thread
        /// This is required otherwise contintinuations will happen on CEF UI threads
        /// </summary>
        /// <typeparam name="TResult">Generic param</typeparam>
        /// <param name="taskCompletionSource">tcs</param>
        /// <param name="result">result</param>
        public static void TrySetResultAsync<TResult>(this TaskCompletionSource<TResult> taskCompletionSource, TResult result)
        {
            Task.Factory.StartNew(delegate
            { taskCompletionSource.TrySetResult(result); }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }
    }
}