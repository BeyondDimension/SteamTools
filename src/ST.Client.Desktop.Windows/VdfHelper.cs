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