// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Integration;

sealed class SwaggerValidValuesAttribute : CustomSwaggerAttribute
{
    public int[]? ValidIntValues { get; set; }

    public string[]? ValidStringValues { get; set; }
}
