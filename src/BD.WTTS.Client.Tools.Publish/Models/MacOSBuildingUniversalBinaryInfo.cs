using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.Client.Tools.Publish.Models;

/// <summary>
/// macOS 构建二进制文件信息
/// </summary>
/// <param name="Arm64DiffX64">Arm64 与 X64 的差异文件相对路径</param>
/// <param name="X64DiffArm64">X64 与 Arm64 的差异文件相对路径</param>
/// <param name="Arm64">扫描的 Arm64 文件信息</param>
/// <param name="X64">扫描的 X64 文件信息</param>
public record class MacOSBuildingUniversalBinaryInfo(
    HashSet<string> Arm64DiffX64,
    HashSet<string> X64DiffArm64,
    AppPublishInfo Arm64,
    AppPublishInfo X64)
{
    public override string ToString()
    {
        var str = JsonSerializer.Serialize(this,
            MacOSBuildingUniversalBinaryInfoContext.Default.MacOSBuildingUniversalBinaryInfo);
        return str;
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(MacOSBuildingUniversalBinaryInfo))]
internal partial class MacOSBuildingUniversalBinaryInfoContext : JsonSerializerContext
{

}