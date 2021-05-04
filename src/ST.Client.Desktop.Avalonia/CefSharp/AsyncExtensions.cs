// Copyright © 2017 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.
// https://github.com/cefsharp/CefSharp/blob/v89.0.170/CefSharp/AsyncExtensions.cs
using CefNet;
using CefNet.Net;
using System;
using System.Threading.Tasks;

namespace CefSharp
{
    public static class AsyncExtensions
    {
        /// <summary>
        /// Sets a cookie given a valid URL and explicit user-provided cookie attributes.
        /// This function expects each attribute to be well-formed. It will check for disallowed
        /// characters (e.g. the ';' character is disallowed within the cookie value attribute) and will return false without setting
        /// </summary>
        /// <param name="cookieManager">cookie manager</param>
        /// <param name="url">The cookie URL. If an empty string is provided, any URL will be matched.</param>
        /// <param name="cookie">the cookie to be set</param>
        /// <returns>returns false if the cookie cannot be set (e.g. if illegal charecters such as ';' are used);
        /// otherwise task that represents the set operation. The value of the TResult parameter contains a bool to indicate success.</returns>
        public static Task<bool> SetCookieAsync(this CefCookieManager cookieManager, string url, CefNetCookie cookie)
        {
            if (cookieManager == null)
            {
                throw new NullReferenceException("cookieManager");
            }

            if (cookieManager.IsDisposed)
            {
                throw new ObjectDisposedException("cookieManager");
            }

            var callback = new TaskSetCookieCallback();
            if (cookieManager.SetCookie(url, cookie, callback))
            {
                return callback.Task;
            }

            //There was a problem setting cookies
            return Task.FromResult(false);
        }
    }
}