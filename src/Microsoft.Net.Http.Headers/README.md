# Microsoft.Net.Http.Headers

## fix net6.0-android & asp.net core 2.x  
```
Error|Microsoft.AspNetCore.Server.Kestrel|Heartbeat.OnHeartbeat | System.TypeLoadException: Could not resolve type with token 01000026 from typeref (expected class 'Microsoft.Extensions.Primitives.InplaceStringBuilder' in assembly 'Microsoft.Extensions.Primitives, Version=2.2.0.0, Culture=neutral, PublicKeyToken=adb9793829ddae60')
   at Microsoft.Net.Http.Headers.HeaderUtilities.FormatDate(DateTimeOffset dateTime, Boolean quoted)
   at Microsoft.Net.Http.Headers.HeaderUtilities.FormatDate(DateTimeOffset dateTime)
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.DateHeaderValueManager.SetDateValues(DateTimeOffset value)
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.DateHeaderValueManager.OnHeartbeat(DateTimeOffset now)
   at Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Infrastructure.Heartbeat.OnHeartbeat()
```

https://github.com/dotnet/aspnetcore/tree/v2.2.13/src/Http/Headers/src