#if ANDROID
[assembly: System.Runtime.Versioning.SupportedOSPlatform("Android21.0")]
#elif WINDOWS
[assembly: System.Runtime.Versioning.SupportedOSPlatform("Windows10.0.17763.0")]
#elif IOS
[assembly: System.Runtime.Versioning.SupportedOSPlatform("iOS10.0")]
#elif MACCATALYST
[assembly: System.Runtime.Versioning.SupportedOSPlatform("maccatalyst13.1")]
#endif