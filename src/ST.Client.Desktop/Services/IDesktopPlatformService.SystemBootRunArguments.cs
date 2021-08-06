namespace System.Application.Services
{
#if NETFRAMEWORK
    static
#endif
    partial
#if NETFRAMEWORK
    class
#else
    interface
#endif
    IDesktopPlatformService
    {
#if NETFRAMEWORK
        public
#endif
        const string SystemBootRunArguments = "-clt c -silence";
    }
}