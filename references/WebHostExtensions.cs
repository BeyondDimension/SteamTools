// https://github.com/aspnet/Hosting/blob/2.2.0/src/Microsoft.AspNetCore.Hosting/WebHostExtensions.cs
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostCompatExtensions
    {
        /// <summary>
        /// Runs a web application and block the calling thread until host shutdown.
        /// </summary>
        /// <param name="host">The <see cref="IWebHost"/> to run.</param>
        public static void RunCompat(this IWebHost host)
        {
            host.RunCompatAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs a web application and returns a Task that only completes when the token is triggered or shutdown is triggered.
        /// </summary>
        /// <param name="host">The <see cref="IWebHost"/> to run.</param>
        /// <param name="token">The token to trigger shutdown.</param>
        public static async Task RunCompatAsync(this IWebHost host, CancellationToken token = default)
        {
            // Wait for token shutdown if it can be canceled
            if (token.CanBeCanceled)
            {
                await host.RunAsync(token, shutdownMessage: null);
                return;
            }

            // If token cannot be canceled, attach Ctrl+C and SIGTERM shutdown
            var done = new ManualResetEventSlim(false);
            using (var cts = new CancellationTokenSource())
            {
                var shutdownMessage = host.Services.GetRequiredService<WebHostOptions>().SuppressStatusMessages ? string.Empty : "Application is shutting down...";
                AttachCtrlcSigtermShutdown(cts, done, shutdownMessage: shutdownMessage);

                try
                {
                    await host.RunAsync(cts.Token, "Application started. Press Ctrl+C to shut down.");
                }
                finally
                {
                    done.Set();
                }
            }
        }

        private static async Task RunAsync(this IWebHost host, CancellationToken token, string shutdownMessage)
        {
            using (host)
            {
                await host.StartAsync(token);

                var hostingEnvironment = host.Services.GetService<IHostingEnvironment>();
                var options = host.Services.GetRequiredService<WebHostOptions>();

                if (!options.SuppressStatusMessages)
                {
                    Console.WriteLine($"Hosting environment: {hostingEnvironment.EnvironmentName}");
                    Console.WriteLine($"Content root path: {hostingEnvironment.ContentRootPath}");

                    var serverAddresses = host.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses;
                    if (serverAddresses != null)
                    {
                        foreach (var address in serverAddresses)
                        {
                            Console.WriteLine($"Now listening on: {address}");
                        }
                    }

                    if (!string.IsNullOrEmpty(shutdownMessage))
                    {
                        Console.WriteLine(shutdownMessage);
                    }
                }

                await host.WaitForTokenShutdownAsync(token);
            }
        }

        private static void AttachCtrlcSigtermShutdown(CancellationTokenSource cts, ManualResetEventSlim resetEvent, string shutdownMessage)
        {
            void Shutdown()
            {
                if (!cts.IsCancellationRequested)
                {
                    if (!string.IsNullOrEmpty(shutdownMessage))
                    {
                        Console.WriteLine(shutdownMessage);
                    }
                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }

                // Wait on the given reset event
                resetEvent.Wait();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            try
            {
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    Shutdown();
                    // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                    eventArgs.Cancel = true;
                };
            }
            catch (PlatformNotSupportedException)
            {
            }
        }

        private static async Task WaitForTokenShutdownAsync(this IWebHost host, CancellationToken token)
        {
            var applicationLifetime = host.Services.GetService<IApplicationLifetime>();

            token.Register(state =>
            {
                ((IApplicationLifetime)state).StopApplication();
            },
            applicationLifetime);

            var waitForStop = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
            applicationLifetime.ApplicationStopping.Register(obj =>
            {
                var tcs = (TaskCompletionSource<object>)obj;
                tcs.TrySetResult(null);
            }, waitForStop);

            await waitForStop.Task;

            // WebHost will use its default ShutdownTimeout if none is specified.
            await host.StopAsync();
        }
    }
}