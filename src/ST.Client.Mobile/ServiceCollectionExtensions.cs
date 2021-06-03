using Microsoft.Identity.Client;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMSALPublicClientApp(this IServiceCollection services, Guid clientId)
        {
            if (clientId != default && DI.Platform == Platform.Android)
            {
                // https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-client-application-configuration#client-id
                var publicClientApp = PublicClientApplicationBuilder.Create(clientId.ToString())
                    .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                    .WithAuthority("https://login.microsoftonline.com/consumers/")
                    .Build();
                _ = services.AddSingleton(publicClientApp);
            }
            return services;
        }
    }
}