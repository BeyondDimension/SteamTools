using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace SteamTool.Core
{
    public class RegistryKeyService
    {
        /// <summary>
        /// 创建注册表项
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public RegistryKey CreateRegistryKey(RegistryKey registryKey, string path)
        {
            return registryKey.CreateSubKey(path);
        }

        /// <summary>
        /// 删除注册表项
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public void DeleteRegistryKey(RegistryKey registryKey, string path)
        {
            registryKey.DeleteSubKey(path, true); //该方法无返回值，直接调用即可
            registryKey.Close();
        }

        /// <summary>
        /// 读取注册表值
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string ReadRegistryKey(RegistryKey registryKey, string path, string key)
        {
            var myreg = registryKey.OpenSubKey(path);

            if (myreg != null)
            {
                var info = myreg.GetValue(key)?.ToString();
                myreg.Close();
                return info;
            }
            return string.Empty;
        }

        /// <summary>
        /// 新增或修改注册表值
        /// </summary>
        /// <param name="registryKey"></param>
        /// <param name="path"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="registryValueKind"></param>
        /// <returns></returns>
        public void AddOrUpdateRegistryKey(RegistryKey registryKey, string path, string key, string value, RegistryValueKind registryValueKind)
        {
            RegistryKey myreg = registryKey.OpenSubKey(path, true); //该项必须已存在
            if (myreg != null)
            {
                myreg.SetValue(key, value, registryValueKind);
                myreg.Close();
            }
        }

    }
}
