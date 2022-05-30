namespace System.Application.Services.Implementation;

sealed class EmailPlatformServiceImpl : IEmailPlatformService
{
    Task IEmailPlatformService.PlatformComposeAsync(EmailMessage? message)
    {
        var message2 = message.Convert();
        return message2 == null ? Email.ComposeAsync() : Email.ComposeAsync(message2);
    }
}
