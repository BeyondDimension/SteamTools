using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  Gameloop.Vdf;
using Gameloop.Vdf.Linq;

namespace SteamTool.Core.Service
{
    public class VdfService
    {


        /// <summary>
        /// 获取VDF类型内容
        /// </summary>
        /// <param name="path">vdf文件内容</param>
        /// <returns></returns>
        public dynamic GetVdfModel(string text)
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
            string text = File.ReadAllText(path,Encoding.UTF8);
            return GetVdfModel(text); ;
        }
    }
}
