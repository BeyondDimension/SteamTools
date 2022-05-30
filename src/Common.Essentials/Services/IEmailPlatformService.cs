namespace System.Application.Services;

public interface IEmailPlatformService
{
    static IEmailPlatformService? Instance => DI.Get_Nullable<IEmailPlatformService>();

    Task PlatformComposeAsync(EmailMessage? message);
}