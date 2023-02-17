// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

public class SteamRemoteFile : ReactiveObject
{
    public string Name { get; private set; }

    public bool Exists { get; }

    public bool IsPersisted { get; }

    public long Size { get; }

    public DateTimeOffset Timestamp { get; }

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    public ERemoteStoragePlatform SyncPlatforms { get; set; }

#endif

    public SteamRemoteFile(string name)
    {
        Name = name;
    }

    public SteamRemoteFile(string name, long length, bool exists, bool isPersisted, long timestamp)
    {
        Name = name;
        Size = length;
        Exists = exists;
        IsPersisted = isPersisted;
        Timestamp = timestamp.ToDateTimeOffsetS();
    }

    public int Read(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileRead(Name, buffer);
    }

    public byte[] ReadAllBytes()
    {
        byte[] buffer = new byte[Size];
        int read = Read(buffer);
        if (read != buffer.Length)
            throw new IOException("Could not read entire file.");
        return buffer;
    }

    public bool WriteAllBytes(byte[] buffer)
    {
        return ISteamworksLocalApiService.Instance.FileWrite(Name, buffer);
    }

    public bool Forget()
    {
        return ISteamworksLocalApiService.Instance.FileForget(Name);
    }

    public bool Delete()
    {
        return ISteamworksLocalApiService.Instance.FileDelete(Name);
    }
}