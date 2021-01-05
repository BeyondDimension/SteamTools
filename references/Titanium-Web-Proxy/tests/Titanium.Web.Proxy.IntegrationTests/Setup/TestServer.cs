using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Titanium.Web.Proxy.IntegrationTests.Setup
{
    // set up a kestrel test server
    public class TestServer : IDisposable
    {
        public string ListeningHttpUrl => $"http://localhost:{HttpListeningPort}";
        public string ListeningHttpsUrl => $"https://localhost:{HttpsListeningPort}";
        public string ListeningTcpUrl => $"http://localhost:{TcpListeningPort}";

        public int HttpListeningPort { get; private set; }
        public int HttpsListeningPort { get; private set; }
        public int TcpListeningPort { get; private set; }

        private IWebHost host;
        public TestServer(X509Certificate2 serverCertificate)
        {
            var startUp = new Startup(() => requestHandler);

            host = WebHost.CreateDefaultBuilder()
                         .ConfigureServices(s =>
                         {
                             s.AddSingleton<IStartup>(startUp);
                         })
                          .UseKestrel(options =>
                          {
                              options.Listen(IPAddress.Loopback, 0);
                              options.Listen(IPAddress.Loopback, 0, listenOptions =>
                              {
                                  listenOptions.UseHttps(serverCertificate);
                              });
                              options.Listen(IPAddress.Loopback, 0, listenOptions =>
                              {
                                  listenOptions.Run(context =>
                                  {
                                      if (tcpRequestHandler == null)
                                      {
                                          throw new Exception("Test server not configured to handle tcp request.");
                                      }

                                      return tcpRequestHandler(context);
                                  });
                              });
                          })
                        .Build();

            host.Start();

            var addresses = host.ServerFeatures
                        .Get<IServerAddressesFeature>()
                        .Addresses.ToArray();

            HttpListeningPort = new Uri(addresses[0]).Port;
            HttpsListeningPort = new Uri(addresses[1]).Port;
            TcpListeningPort = new Uri(addresses[2]).Port;
        }

        Func<HttpContext, Task> requestHandler = null;
        Func<ConnectionContext, Task> tcpRequestHandler = null;

        public void HandleRequest(Func<HttpContext, Task> requestHandler)
        {
            this.requestHandler = requestHandler;
        }

        public void HandleTcpRequest(Func<ConnectionContext, Task> tcpRequestHandler)
        {
            this.tcpRequestHandler = tcpRequestHandler;
        }

        public void Dispose()
        {
            host.StopAsync().Wait();
            host.Dispose();
        }

        private class Startup : IStartup
        {
            Func<Func<HttpContext, Task>> requestHandler;
            public Startup(Func<Func<HttpContext, Task>> requestHandler)
            {
                this.requestHandler = requestHandler;
            }

            public void Configure(IApplicationBuilder app)
            {
                app.Run(context =>
                {
                    if (requestHandler == null)
                    {
                        throw new Exception("Test server not configured to handle request.");
                    }

                    return requestHandler()(context);
                });

            }

            public IServiceProvider ConfigureServices(IServiceCollection services)
            {
                return services.BuildServiceProvider();
            }
        }
    }
}
