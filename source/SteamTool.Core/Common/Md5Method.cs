using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SteamTool.Core.Common
{
    /// <summary>
    /// 类名    ：MD5加密<br/>
    /// 说明    ：<br/>
    /// </summary>
    [Obsolete("use System.Security.Cryptography.Hashs.String(ByteArray).MD5", true)]
    public static class Md5Method
    {
        /// <summary>
        /// 用MD5加密一段字符串
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="isLower">是否小写</param>
        /// <returns>MD5串</returns>
        public static string MD5Encrypt(string str, bool isLower = false)
        {
            using (var md5 = MD5.Create())
            {
                var result = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                var strResult = BitConverter.ToString(result).Replace("-", "");
                return isLower ? strResult.ToLower() : strResult;
            }
        }

        /// <summary>
        /// 用MD5加密流
        /// </summary>
        /// <param name="myStream">流</param>
        /// <returns>str</returns>
        public static byte[] MD5Encrypt(Stream inputStream)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return md5.ComputeHash(inputStream);
        }

        /// <summary>
        /// 用MD5加密字节数组
        /// </summary>
        /// <param name="myBytes">字节数组</param>
        /// <returns></returns>
        public static byte[] MD5Encrypt(byte[] myBytes)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            return md5.ComputeHash(myBytes, 0, myBytes.Length);
        }

        /// <summary>
        /// 文件路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FileMD5Encrypt(string path)
        {
            using FileStream fs = new FileStream(path, FileMode.Open);
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] data = provider.ComputeHash(fs);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("X2"));
            }
            return builder.ToString();
        }
    }
}
