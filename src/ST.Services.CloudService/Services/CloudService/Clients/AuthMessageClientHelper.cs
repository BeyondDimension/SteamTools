using System.Application.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    public static class AuthMessageClientHelper
    {
        static IEnumerable<string> QueryClearKeys(IDictionary<string, DateTime> pairs, DateTime now)
        {
            foreach (var item in pairs)
            {
                if (!((now - item.Value).TotalSeconds <= Constants.SMSIntervalActually))
                {
                    yield return item.Key;
                }
            }
        }

        static void Clear(IDictionary<string, DateTime> pairs, DateTime now)
        {
            var keys = QueryClearKeys(pairs, now).ToArray();
            foreach (var item in keys)
            {
                pairs.Remove(item);
            }
        }

        /// <summary>
        /// 检查在已发送的时间间隔内，则不调用http，直接返回OK
        /// </summary>
        /// <param name="pairs"></param>
        /// <param name="mark"></param>
        /// <returns></returns>
        static bool CheckInterval(DateTime now, IDictionary<string, DateTime> pairs, string mark)
        {
            Clear(pairs, now);
            if (pairs.TryGetValue(mark, out var value))
            {
                if ((now - value).TotalSeconds <= Constants.SMSIntervalActually)
                {
                    return true;
                }
            }
            return false;
        }

        static string GetMark(SendSmsRequest request)
           => request.PhoneNumber + request.Type;

        static void SetMark(DateTime now, IDictionary<string, DateTime> pairs, string mark)
        {
            if (pairs.ContainsKey(mark))
            {
                pairs[mark] = now;
            }
            else
            {
                pairs.Add(mark, now);
            }
        }

        public static IDictionary<string, DateTime> Create()
            => new ConcurrentDictionary<string, DateTime>();

        public static async ValueTask<IApiResponse> SendSms(
            IDictionary<string, DateTime> pairs,
            SendSmsRequest request,
            Func<SendSmsRequest, Task<IApiResponse>> func)
        {
            var now = DateTime.Now;
            var mark = GetMark(request);
            if (CheckInterval(now, pairs, mark))
            {
                return ApiResponse.Ok();
            }
            var r = await func(request);
            if (r.IsSuccess)
            {
                SetMark(now, pairs, mark);
            }
            return r;
        }
    }
}