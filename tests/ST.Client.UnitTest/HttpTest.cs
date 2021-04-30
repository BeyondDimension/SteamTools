using NUnit.Framework;
using System.Application.Models;
using System.Application.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application
{
    [TestFixture]
    public class HttpTest
    {
        /// <summary>
        /// 测试图片下载缓存，同一个图片地址，多次请求，仅发送一次，且优先从缓存中加载图片
        /// </summary>
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

#if DEBUG

        ///// <summary>
        ///// 测试 登陆与注册 接口，先开服务端(私有)，再测试
        ///// </summary>
        ///// <returns></returns>
        //[Test]
        //public async Task LoginOrRegister()
        //{
        //    var client = ICloudServiceClient.Instance;

        //    var req1 = new SendSmsRequest
        //    {
        //        PhoneNumber = "18611112222",
        //        Type = SmsCodeType.LoginOrRegister,
        //    };

        //    var rsp1 = await client.AuthMessage.SendSms(req1);

        //    Assert.IsTrue(rsp1.IsSuccess);

        //    var req2 = new LoginOrRegisterRequest
        //    {
        //        PhoneNumber = req1.PhoneNumber,
        //        SmsCode = "666666",
        //    };
        //    var rsp2 = await ICloudServiceClient.Instance.Account.LoginOrRegister(req2);

        //    Assert.IsTrue(rsp2.IsSuccess);

        //    var isLoginOrRegister = rsp2.Content.ThrowIsNull(nameof(rsp2.Content)).IsLoginOrRegister;

        //    TestContext.WriteLine($"isLoginOrRegister: {isLoginOrRegister}");

        //    var jsonStr = Serializable2.S(rsp2.Content);

        //    TestContext.WriteLine("jsonStr: ");
        //    TestContext.WriteLine(jsonStr);
        //}

#endif
    }
}