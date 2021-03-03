using NUnit.Framework;
using System.Application.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application
{
    [TestFixture]
    public class HttpTest
    {
        [Test]
        public void GetImage()
        {
            const string api_base = "https://localhost:5001/";
            var images = new[] {
                "images/icons/company.png",
                "images/icons/GitHub-Mark-120px-plus.png",
                "images/icons/icon.png",
                "images/company.png",
                "images/icons/company.png",
                "images/icons/GitHub-Mark-120px-plus.png",
                "images/icons/logo.png",
                "images/company.png",
                "images/icons/icon.png",
                "images/icons/icon.png",
                "images/icons/steam.png" };

            var tasks = images.Select(Handle).ToArray();
            Task.WaitAll(tasks);
            TestContext.WriteLine("OK");

            static async Task Handle(string requestUri)
            {
                var tid = Thread.CurrentThread.ManagedThreadId;
                var result = await IHttpService.Instance.GetImageAsync(
                   api_base + requestUri, ImageChannelType.SteamAvatars);
                var tid2 = Thread.CurrentThread.ManagedThreadId;
                TestContext.WriteLine($"({tid}-{tid2})requestUri: {requestUri}, length: {result?.Length}");
            }
        }
    }
}