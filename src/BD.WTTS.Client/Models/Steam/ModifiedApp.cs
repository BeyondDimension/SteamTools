#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

[MPObj, MP2Obj(SerializeLayout.Explicit)]
public partial class ModifiedApp
{
    [MPConstructor, MP2Constructor]
    public ModifiedApp()
    {
        ReadChanges();
    }

    public ModifiedApp(SteamApp app)
    {
        if (app.ChangesData == null)
        {
            throw new ArgumentException("New ModifiedApp Failed. SteamApp.ChangesData is null.");
        }
        AppId = app.AppId;
        if (app.OriginalData?.Clone() is byte[] originalData)
        {
            OriginalData = originalData;
        }

        using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
        binaryWriter.Write(app.ChangesData);

        ChangesData = binaryWriter.BaseStream.ToByteArray();
    }

    [MPKey(0), MP2Key(0)]
    public uint AppId { get; set; }

    private byte[]? originalData;

    [MPKey(1), MP2Key(1)]
    public byte[]? OriginalData { get => originalData; set => originalData = value; }

    private byte[]? changesData;

    [MPKey(2), MP2Key(2)]
    public byte[]? ChangesData { get => changesData; set => changesData = value; }

    [MPIgnore, MP2Ignore]
    public SteamAppPropertyTable? Changes { get; set; }

    public SteamAppPropertyTable? ReadChanges()
    {
        if (ChangesData != null)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(ChangesData));
            return Changes = reader.ReadPropertyTable();
        }
        return null;
    }

    public SteamAppPropertyTable? ReadOriginalData()
    {
        if (OriginalData != null)
        {
            using BinaryReader reader = new BinaryReader(new MemoryStream(OriginalData));
            return reader.ReadPropertyTable();
        }
        return null;
    }
}
#endif