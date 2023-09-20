// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.IPC.Integration;

sealed class SwaggerSteamIdentifierAttribute : CustomSwaggerAttribute
{
    public EAccountType AccountType { get; set; } = EAccountType.Individual;

    public uint MaximumAccountID { get; set; } = uint.MaxValue;

    public uint MinimumAccountID { get; set; } = 1;

    public EUniverse Universe { get; set; } = EUniverse.Public;
}
