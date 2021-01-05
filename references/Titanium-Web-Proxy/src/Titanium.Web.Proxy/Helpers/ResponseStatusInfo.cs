using System;

namespace Titanium.Web.Proxy.Helpers
{
    struct ResponseStatusInfo
    {
        public Version Version { get; set; }

        public int StatusCode { get; set; }

        public string Description { get; set; }
    }
}
