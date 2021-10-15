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
    IPlatformService
    {
#if NETFRAMEWORK
        public
#endif
        const string SystemBootRunArguments = "-clt c -silence";
    }
}