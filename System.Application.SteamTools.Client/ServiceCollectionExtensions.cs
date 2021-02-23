using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServiceCollectionExtensions
    {
        public static IServiceCollection TryAddUserManager(this IServiceCollection services)
        {
            services.TryAddSingleton<ISecurityService, SecurityService>();
            services.TryAddSingleton<IUserManager, UserManager>();
            return services;
        }
    }
}
