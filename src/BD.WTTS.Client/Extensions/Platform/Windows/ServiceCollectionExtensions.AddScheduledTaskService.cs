#if WINDOWS
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [BD.Mobius(
"""
Mobius.Helpers.WinScheduledTaskHelper
""", Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddScheduledTaskService(this IServiceCollection services)
    {
        services.AddSingleton<IScheduledTaskService, ScheduledTaskServiceImpl>();
        return services;
    }
}
#endif