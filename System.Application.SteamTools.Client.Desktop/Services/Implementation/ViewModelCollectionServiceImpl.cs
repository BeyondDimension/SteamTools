using Microsoft.Extensions.DependencyInjection;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.Services.Implementation
{
    [Obsolete("Languages")]
    internal sealed class ViewModelCollectionServiceImpl : IViewModelCollectionService
    {
        readonly IEnumerable<ServiceDescriptor> services;

        public ViewModelCollectionServiceImpl(IEnumerable<ServiceDescriptor> services)
        {
            this.services = services;
        }

        public IEnumerable<ViewModelBase> ViewModels => GetViewModels<ViewModelBase>();

        public IEnumerable<T> GetViewModels<T>()
        {
            var tType = typeof(T);
            var vType = typeof(ViewModelBase);
            var tIsvType = tType == vType;
            return from s in services
                   where s.ServiceType == s.ImplementationType &&
                   s.Lifetime == ServiceLifetime.Singleton &&
                   tType.IsAssignableFrom(s.ImplementationType) &&
                   (tIsvType || vType.IsAssignableFrom(s.ImplementationType))
                   select (T)DI.Get(s.ServiceType);
        }
    }
}