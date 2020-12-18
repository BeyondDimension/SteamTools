using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gameloop.Vdf;
using Gameloop.Vdf.Linq;

namespace SteamTool.Core
{
    public class VdfService
    {
        /// <summary>
        /// 获取VDF类型内容
        /// </summary>
        /// <param name="path">vdf文件内容</param>
        /// <returns></returns>
        public VProperty GetVdfModel(string text)
        {
            return VdfConvert.Deserialize(text);
        }

        /// <summary>
        /// 根据路径获取VDF类型内容
        /// </summary>
        /// <param name="path">vdf文件路径</param>
        /// <returns></returns>
        public dynamic GetVdfModelByPath(string path)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            return GetVdfModel(text);
        }


        public void DeleteVdfValueByKey(string path, string key)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            //如此实现的功能非常简单，但是应付嵌套的格式时会有问题
            var index = text.IndexOf($"\"{key}\"");
            var lastIndex = text.IndexOf('}', index);
            text = text.Remove(index, lastIndex - index + 1);
            //Debug.WriteLine(text);
            File.WriteAllText(path, text, Encoding.UTF8);
        }
    }
}
