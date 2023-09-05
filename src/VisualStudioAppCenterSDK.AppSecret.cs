static partial class VisualStudioAppCenterSDK
{
#if APP_REVERSE_PROXY
    static string AppSecret
    {
        get
        {
            if (OperatingSystem.IsWindows())
            {
#if DEBUG
                return "ecbed6ce-122e-47ba-b72b-b1321e908b24";
#else
                return "aa571e00-8cc8-4994-8331-1dc56b361142";
#endif
            }
            else if (OperatingSystem.IsMacOS())
            {
#if DEBUG
                return "b3aa4e6e-8570-4af0-8f6e-4f14beecbdcb";
#else
                return "ca51b11c-0288-48b0-9c59-868a3cc30cd6";
#endif
            }
            else if (OperatingSystem.IsLinux())
            {
#if DEBUG
                return "3946531b-fb24-41f9-88c9-da4ca01794a1";
#else
                return "8add2327-6a60-456a-ada1-f77d31d502e3";
#endif
            }
            return "";
        }
    }
#else
    const string AppSecret =
#if DEBUG
#if WINDOWS
        "ccca922e-40fe-48ab-9982-45ba496b1201";
#elif ANDROID
        "f001be5b-32f2-4c6f-84e2-faf6ece68a32";
#elif IOS
        "4485f151-9b20-480c-844a-34bf2764e7c6";
#elif MACOS
        "b55afe65-22fb-484f-aded-4e47fa801a94";
#elif MACCATALYST
        "";
#elif LINUX
        "08669c8c-a82d-4f3d-ba4b-d3ab1dfd3294";
#else
        "";
#endif
#else
#if WINDOWS
        "fe4173e4-c4db-464c-958e-0296b84c0a59";
#elif ANDROID
        "abb40a14-8c4d-4c36-9a2c-f4f819b7080e";
#elif IOS
        "132c565f-16d2-42f8-b980-7ed885e34e89";
#elif MACOS
        "225e5aa3-b6da-4bdc-a9d2-7c357f4c2b7b";
#elif MACCATALYST
        "";
#elif LINUX
        "abca614f-ec64-49fc-86d2-24abb7525140";
#else
        "";
#endif
#endif
#endif
}