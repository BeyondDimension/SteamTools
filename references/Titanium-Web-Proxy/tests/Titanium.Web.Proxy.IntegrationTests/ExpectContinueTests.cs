using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.IntegrationTests.Helpers;

namespace Titanium.Web.Proxy.IntegrationTests
{
    [TestClass]
    public class ExpectContinueTests
    {
        [TestMethod]
        public async Task ReverseProxy_GotContinueAndOkResponse()
        {
            var testSuite = new TestSuite();
            var server = testSuite.GetServer();
            var continueServer = new HttpContinueServer
            {
                ExpectationResponse = HttpStatusCode.Continue,
                ResponseBody = "I am server. I received your greetings."
            };
            server.HandleTcpRequest(continueServer.HandleRequest);

            var proxy = testSuite.GetReverseProxy();
            proxy.Enable100ContinueBehaviour = true;
            proxy.BeforeRequest += (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningTcpUrl;
                return Task.CompletedTask;
            };

            var client = new HttpContinueClient();
            var response = await client.Post("localhost", proxy.ProxyEndPoints[0].Port, "Hello server. I am a client.");

            Assert.IsNotNull(response, "No response to 'expect: 100-continue' request");
            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(continueServer.ResponseBody, response.BodyString);
        }

        [TestMethod]
        public async Task ReverseProxy_GotExpectationFailedResponse()
        {
            var testSuite = new TestSuite();
            var server = testSuite.GetServer();
            var continueServer = new HttpContinueServer { ExpectationResponse = HttpStatusCode.ExpectationFailed };
            server.HandleTcpRequest(continueServer.HandleRequest);

            var proxy = testSuite.GetReverseProxy();
            proxy.Enable100ContinueBehaviour = true;
            proxy.BeforeRequest += (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningTcpUrl;
                return Task.CompletedTask;
            };

            var client = new HttpContinueClient();
            var response = await client.Post("localhost", proxy.ProxyEndPoints[0].Port, "Hello server. I am a client.");

            Assert.IsNotNull(response, "No response to 'expect: 100-continue' request");
            Assert.AreEqual((int)HttpStatusCode.ExpectationFailed, response.StatusCode);
        }

        [TestMethod]
        public async Task ReverseProxy_GotNotFoundResponse()
        {
            var testSuite = new TestSuite();
            var server = testSuite.GetServer();
            var continueServer = new HttpContinueServer { ExpectationResponse = HttpStatusCode.NotFound };
            server.HandleTcpRequest(continueServer.HandleRequest);

            var proxy = testSuite.GetReverseProxy();
            proxy.Enable100ContinueBehaviour = true;
            proxy.BeforeRequest += (sender, e) =>
            {
                e.HttpClient.Request.Url = server.ListeningTcpUrl;
                return Task.CompletedTask;
            };

            var client = new HttpContinueClient();
            var response = await client.Post("localhost", proxy.ProxyEndPoints[0].Port, "Hello server. I am a client.");

            Assert.IsNotNull(response, "No response to 'expect: 100-continue' request");
            Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ReverseProxy_BeforeRequestThrows()
        {
            var testSuite = new TestSuite();
            var server = testSuite.GetServer();
            var continueServer = new HttpContinueServer { ExpectationResponse = HttpStatusCode.Continue };
            server.HandleTcpRequest(continueServer.HandleRequest);

            var dbzEx = new DivideByZeroException("Undefined");
            var dbzString = $"{dbzEx.GetType()}: {dbzEx.Message}";

            var proxy = testSuite.GetReverseProxy();
            proxy.Enable100ContinueBehaviour = true;
            proxy.BeforeRequest += (sender, e) =>
            {
                try
                {
                    e.HttpClient.Request.Url = server.ListeningTcpUrl;
                    throw dbzEx;
                }
                catch
                {
                    var serverError = new Response(Encoding.ASCII.GetBytes(dbzString))
                    {
                        HttpVersion = new Version(1, 1),
                        StatusCode = (int)HttpStatusCode.InternalServerError,
                        StatusDescription = HttpStatusCode.InternalServerError.ToString()
                    };

                    e.Respond(serverError);
                }

                return Task.CompletedTask;
            };

            var client = new HttpContinueClient();
            var response = await client.Post("localhost", proxy.ProxyEndPoints[0].Port, "Hello server. I am a client.");

            Assert.IsNotNull(response, "No response to 'expect: 100-continue' request");
            Assert.AreEqual(response.StatusCode, (int)HttpStatusCode.InternalServerError);
            Assert.AreEqual(response.BodyString, dbzString);
        }
    }
}
