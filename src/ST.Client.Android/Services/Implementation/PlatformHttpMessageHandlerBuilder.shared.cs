using Microsoft.Extensions.Http;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace System.Application.Services.Implementation
{
    /// <summary>
    /// https://github.com/dotnet/runtime/blob/v5.0.4/src/libraries/Microsoft.Extensions.Http/src/DefaultHttpMessageHandlerBuilder.cs
    /// </summary>
    internal sealed partial class PlatformHttpMessageHandlerBuilder : HttpMessageHandlerBuilder
    {
        public PlatformHttpMessageHandlerBuilder(IServiceProvider services)
        {
            Services = services;
        }

        string? _name;

        [DisallowNull]
        public override string? Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override HttpMessageHandler PrimaryHandler { get; set; } = CreateHttpMessageHandler();

        public override IList<DelegatingHandler> AdditionalHandlers { get; } = new List<DelegatingHandler>();

        public override IServiceProvider Services { get; }

        public override HttpMessageHandler Build()
        {
            //if (PrimaryHandler == null)
            //{
            //    const string message = "PrimaryHandler == null(PlatformHttpMessageHandlerBuilder.Build)";
            //    throw new InvalidOperationException(message);
            //}

            return CreateHandlerPipeline(PrimaryHandler, AdditionalHandlers);
        }
    }
}