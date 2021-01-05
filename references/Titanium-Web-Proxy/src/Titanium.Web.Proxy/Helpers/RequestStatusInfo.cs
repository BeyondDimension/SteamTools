using System;
using Titanium.Web.Proxy.Models;

namespace Titanium.Web.Proxy.Helpers
{
    struct RequestStatusInfo
    {
        public string Method { get; set; }

        public ByteString RequestUri { get; set; }

        public Version Version { get; set; }

        public bool IsEmpty()
        {
            return Method == null && RequestUri.Length == 0 && Version == null;
        }
    }
}
