#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

public sealed class ImportedSDAEntry
{
    public const int PBKDF2_ITERATIONS = 50000;
    public const int SALT_LENGTH = 8;
    public const int KEY_SIZE_BYTES = 32;
    public const int IV_LENGTH = 16;

    public string? Username;
    public string? SteamId;
    public string? json;

    public override string ToString()
    {
        return Username + " (" + SteamId + ")";
    }
}