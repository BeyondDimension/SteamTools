#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// Valve Data File 格式助手类
/// </summary>
public static class VdfHelper
{
    /// <summary>
    /// 根据路径读取 Valve Data File 内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static VProperty Read(string filePath)
    {
        var text = File.ReadAllText(filePath, Encoding.UTF8);
        return VdfConvert.Deserialize(text);
    }

    public static void Write(string filePath, VProperty content)
    {
        try
        {
            File.WriteAllText(filePath, VdfConvert.Serialize(content), Encoding.UTF8);
        }
        catch (Exception e)
        {
            Log.Error(nameof(VdfHelper), e, "Write Vdf Error");
        }
    }
}
#endif