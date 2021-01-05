using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.UnitTests
{
    [TestClass]
    public class ProxyServerTests
    {
        [TestMethod]
        public void
            GivenOneEndpointIsAlreadyAddedToAddress_WhenAddingNewEndpointToExistingAddress_ThenExceptionIsThrown()
        {
            // Arrange
            var proxy = new ProxyServer();
            const int port = 9999;
            var firstIpAddress = IPAddress.Parse("127.0.0.1");
            var secondIpAddress = IPAddress.Parse("127.0.0.1");
            proxy.AddEndPoint(new ExplicitProxyEndPoint(firstIpAddress, port, false));

            // Act
            try
            {
                proxy.AddEndPoint(new ExplicitProxyEndPoint(secondIpAddress, port, false));
            }
            catch (Exception exc)
            {
                // Assert
                StringAssert.Contains(exc.Message, "Cannot add another endpoint to same port");
                return;
            }

            Assert.Fail("An exception should be thrown by now");
        }

        [TestMethod]
        public void
            GivenOneEndpointIsAlreadyAddedToAddress_WhenAddingNewEndpointToExistingAddress_ThenTwoEndpointsExists()
        {
            // Arrange
            var proxy = new ProxyServer();
            const int port = 9999;
            var firstIpAddress = IPAddress.Parse("127.0.0.1");
            var secondIpAddress = IPAddress.Parse("192.168.1.1");
            proxy.AddEndPoint(new ExplicitProxyEndPoint(firstIpAddress, port, false));

            // Act
            proxy.AddEndPoint(new ExplicitProxyEndPoint(secondIpAddress, port, false));

            // Assert
            Assert.AreEqual(2, proxy.ProxyEndPoints.Count);
        }

        [TestMethod]
        public void GivenOneEndpointIsAlreadyAddedToPort_WhenAddingNewEndpointToExistingPort_ThenExceptionIsThrown()
        {
            // Arrange
            var proxy = new ProxyServer();
            const int port = 9999;
            proxy.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Loopback, port, false));

            // Act
            try
            {
                proxy.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Loopback, port, false));
            }
            catch (Exception exc)
            {
                // Assert
                StringAssert.Contains(exc.Message, "Cannot add another endpoint to same port");
                return;
            }

            Assert.Fail("An exception should be thrown by now");
        }

        [TestMethod]
        public void
            GivenOneEndpointIsAlreadyAddedToZeroPort_WhenAddingNewEndpointToExistingPort_ThenTwoEndpointsExists()
        {
            // Arrange
            var proxy = new ProxyServer();
            const int port = 0;
            proxy.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Loopback, port, false));

            // Act
            proxy.AddEndPoint(new ExplicitProxyEndPoint(IPAddress.Loopback, port, false));

            // Assert
            Assert.AreEqual(2, proxy.ProxyEndPoints.Count);
        }
    }
}
