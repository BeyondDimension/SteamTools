using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Titanium.Web.Proxy.IntegrationTests
{
    [TestClass]
    public class InterceptionTests
    {
        [TestMethod]
        public async Task Can_Intercept_Get_Requests()
        {
            var testSuite = new TestSuite();

            bool serverCalled = false;

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                serverCalled = true;
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                if (e.HttpClient.Request.Url.Contains("localhost"))
                {
                    e.Ok("<html><body>TitaniumWebProxy-Stopped!!</body></html>");
                    return;
                }

                await Task.FromResult(0);
            };

            var client = testSuite.GetClient(proxy);

            var response = await client.GetAsync(new Uri(server.ListeningHttpUrl));

            Assert.IsFalse(serverCalled, "Server should not be called.");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("TitaniumWebProxy-Stopped!!"));
        }

        [TestMethod]
        public async Task Can_Intercept_Post_Requests()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                if (e.HttpClient.Request.Url.Contains("localhost"))
                {
                    e.Ok("<html><body>TitaniumWebProxy-Stopped!!</body></html>");
                    return;
                }

                await Task.FromResult(0);
            };

            var client = testSuite.GetClient(proxy);

            var response = await client.PostAsync(new Uri(server.ListeningHttpUrl),
                                        new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("TitaniumWebProxy-Stopped!!"));
        }

        [TestMethod]
        public async Task Can_Intercept_Put_Requests()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                if (e.HttpClient.Request.Url.Contains("localhost"))
                {
                    e.Ok("<html><body>TitaniumWebProxy-Stopped!!</body></html>");
                    return;
                }

                await Task.FromResult(0);
            };

            var client = testSuite.GetClient(proxy);

            var response = await client.PutAsync(new Uri(server.ListeningHttpUrl),
                                                    new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("TitaniumWebProxy-Stopped!!"));
        }


        [TestMethod]
        public async Task Can_Intercept_Patch_Requests()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                if (e.HttpClient.Request.Url.Contains("localhost"))
                {
                    e.Ok("<html><body>TitaniumWebProxy-Stopped!!</body></html>");
                    return;
                }

                await Task.FromResult(0);
            };

            var client = testSuite.GetClient(proxy);

            var response = await client.PatchAsync(new Uri(server.ListeningHttpUrl),
                                                    new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("TitaniumWebProxy-Stopped!!"));
        }

        [TestMethod]
        public async Task Can_Intercept_Delete_Requests()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                if (e.HttpClient.Request.Url.Contains("localhost"))
                {
                    e.Ok("<html><body>TitaniumWebProxy-Stopped!!</body></html>");
                    return;
                }

                await Task.FromResult(0);
            };

            var client = testSuite.GetClient(proxy);

            var response = await client.DeleteAsync(new Uri(server.ListeningHttpUrl));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("TitaniumWebProxy-Stopped!!"));
        }
    }
}
