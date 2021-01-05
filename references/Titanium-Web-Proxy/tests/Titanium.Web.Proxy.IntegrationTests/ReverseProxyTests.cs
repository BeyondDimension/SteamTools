using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Titanium.Web.Proxy.IntegrationTests
{
    [TestClass]
    public class ReverseProxyTests
    {
        [TestMethod]
        public async Task Smoke_Test_Http_To_Http_Reverse_Proxy()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetReverseProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningHttpUrl;
                await Task.FromResult(0);
            };

            var client = testSuite.GetReverseProxyClient();

            var response = await client.PostAsync(new Uri($"http://localhost:{proxy.ProxyEndPoints[0].Port}"),
                                        new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual("I am server. I received your greetings.", body);
        }

        [TestMethod]
        public async Task Smoke_Test_Https_To_Http_Reverse_Proxy()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetReverseProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningHttpUrl;
                await Task.FromResult(0);
            };

            var client = testSuite.GetReverseProxyClient();

            var response = await client.PostAsync(new Uri($"https://localhost:{proxy.ProxyEndPoints[0].Port}"),
                                        new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual("I am server. I received your greetings.", body);
        }

        [TestMethod]
        public async Task Smoke_Test_Http_To_Https_Reverse_Proxy()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetReverseProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningHttpsUrl;
                await Task.FromResult(0);
            };

            var client = testSuite.GetReverseProxyClient();

            var response = await client.PostAsync(new Uri($"http://localhost:{proxy.ProxyEndPoints[0].Port}"),
                                        new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual("I am server. I received your greetings.", body);
        }

        [TestMethod]
        public async Task Smoke_Test_Https_To_Https_Reverse_Proxy()
        {
            var testSuite = new TestSuite();

            var server = testSuite.GetServer();
            server.HandleRequest((context) =>
            {
                return context.Response.WriteAsync("I am server. I received your greetings.");
            });

            var proxy = testSuite.GetReverseProxy();
            proxy.BeforeRequest += async (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningHttpsUrl;
                await Task.FromResult(0);
            };

            var client = testSuite.GetReverseProxyClient();

            var response = await client.PostAsync(new Uri($"https://localhost:{proxy.ProxyEndPoints[0].Port}"),
                                        new StringContent("hello server. I am a client."));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();

            Assert.AreEqual("I am server. I received your greetings.", body);
        }

    }
}
