using Gameloop.Vdf;
using Gameloop.Vdf.Linq;
using System.IO;
using System.Text;

namespace System.Application
{
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
        public static void UpdateValueByReplaceNoPattern(string filePath, string oldVaule, string newValue)
        {
            var text = File.ReadAllText(filePath, Encoding.UTF8);
            text = text.Replace(oldVaule, newValue);
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }
        public static void UpdateValueByReplace(string filePath, string oldVaule, string newValue)
        {
            var text = File.ReadAllText(filePath, Encoding.UTF8);
            text = text.RemovePattern().Replace(
                oldVaule.RemovePattern().Replace(" ", string.Empty), newValue);
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }

        public static void DeleteValueByKey(string filePath, string key)
        {
            var text = File.ReadAllText(filePath, Encoding.UTF8);
            // 如此实现的功能非常简单，但是应付嵌套的格式时会有问题
            var index = text.IndexOf($"\"{key}\"");
            var lastIndex = text.IndexOf('}', index);
            text = text.Remove(index, lastIndex - index + 1);
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }

        public static void DeleteValueByReplace(string filePath, string oldVaule)
        {
            var text = File.ReadAllText(filePath, Encoding.UTF8);
            text = text.Replace(oldVaule, string.Empty);
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }
    }

#if DEBUG

    [Obsolete("use VdfHelper", true)]
    public class VdfService
    {
        [Obsolete("use VdfHelper.UpdateValueByReplace", true)]
        public void UpdateVdfValueByReplace(string path, string oldVaule, string newValue)
        {
        }

        [Obsolete("use VdfHelper.DeleteValueByKey", true)]
        public void DeleteVdfValueByKey(string path, string key)
        {
        }

        [Obsolete("use VdfHelper.DeleteValueByReplace", true)]
        public void DeleteVdfValueByReplace(string path, string oldVaule)
        {
        }
    }

#endif
}