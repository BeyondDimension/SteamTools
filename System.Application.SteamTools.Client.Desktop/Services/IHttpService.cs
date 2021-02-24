using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface IHttpService
    {
        public static IHttpService Instance => DI.Get<IHttpService>();

        Task<T?> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default) where T : notnull;
    }

#if DEBUG

    [Obsolete("use IHttpService.Instance", true)]
    public class HttpServices
    {
    }

#endif
}