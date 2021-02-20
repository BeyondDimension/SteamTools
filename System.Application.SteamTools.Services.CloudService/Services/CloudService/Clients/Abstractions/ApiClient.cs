namespace System.Application.Services.CloudService.Clients.Abstractions
{
    internal abstract class ApiClient
    {
        protected readonly IApiConnection conn;

        public ApiClient(IApiConnection conn)
        {
            this.conn = conn;
        }
    }
}