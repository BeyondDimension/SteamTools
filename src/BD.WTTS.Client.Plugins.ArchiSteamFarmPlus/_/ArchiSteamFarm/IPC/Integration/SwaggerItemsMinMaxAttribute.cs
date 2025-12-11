// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Integration;

sealed class SwaggerItemsMinMaxAttribute : CustomSwaggerAttribute
{
    public uint MaximumUint
    {
        get => BackingMaximum.HasValue ? decimal.ToUInt32(BackingMaximum.Value) : default;
        set => BackingMaximum = value;
    }

    public uint MinimumUint
    {
        get => BackingMinimum.HasValue ? decimal.ToUInt32(BackingMinimum.Value) : default;
        set => BackingMinimum = value;
    }

    private decimal? BackingMaximum;
    private decimal? BackingMinimum;
}
