// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub/AppHostedService.cs

using Microsoft.Extensions.Hosting;

namespace System.Application.Services.Implementation;

sealed class AppHostedService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(1d), stoppingToken);
    }
}
