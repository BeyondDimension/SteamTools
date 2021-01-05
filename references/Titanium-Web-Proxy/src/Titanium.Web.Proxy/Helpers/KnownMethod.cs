namespace Titanium.Web.Proxy.Helpers
{
    internal enum KnownMethod
    {
        Unknown,
        Invalid,

        // RFC 7231: Hypertext Transfer Protocol (HTTP/1.1): Semantics and Content
        Connect,
        Delete,
        Get,
        Head,
        Options,
        Post,
        Put,
        Trace,

        // RFC 7540: Hypertext Transfer Protocol Version 2
        Pri,

        // RFC 5789: PATCH Method for HTTP
        Patch,

        // RFC 3744: Web Distributed Authoring and Versioning (WebDAV) Access Control Protocol
        Acl,

        // RFC 3253: Versioning Extensions to WebDAV (Web Distributed Authoring and Versioning)
        BaselineControl,
        Checkin,
        Checkout,
        Label,
        Merge,
        Mkactivity,
        Mkworkspace,
        Report,
        Unckeckout,
        Update,
        VersionControl,

        // RFC 3648: Web Distributed Authoring and Versioning (WebDAV) Ordered Collections Protocol
        Orderpatch,

        // RFC 4437: Web Distributed Authoring and Versioning (WebDAV): Redirect Reference Resources
        Mkredirectref,
        Updateredirectref,

        // RFC 4791: Calendaring Extensions to WebDAV (CalDAV)
        Mkcalendar,

        // RFC 4918: HTTP Extensions for Web Distributed Authoring and Versioning (WebDAV)
        Copy,
        Lock,
        Mkcol,
        Move,
        Propfind,
        Proppatch,
        Unlock,

        // RFC 5323: Web Distributed Authoring and Versioning (WebDAV) SEARCH
        Search,

        // 	RFC 5842: Binding Extensions to Web Distributed Authoring and Versioning (WebDAV)
        Bind,
        Rebind,
        Unbind,

        // Internet Draft snell-link-method: HTTP Link and Unlink Methods
        Link,
        Unlink,
    }
}
