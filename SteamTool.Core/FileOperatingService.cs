using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SteamTool.Core
{
    public class FileOperatingService
    {

        private void UpdateHosts(string ip, string domain)
        {
            string path = @"C:\WINDOWS\system32\drivers\etc\hosts";

            //更改属性
            File.SetAttributes(path, FileAttributes.Normal);

            //避免重复写入
            string data = File.ReadAllText(path, Encoding.Default);
            if (data.Contains(ip) && data.Contains(domain))
            {
                File.SetAttributes(path, FileAttributes.ReadOnly);
                return;
            }

            try
            {
                //写入为追加模式
                StreamWriter sw = new StreamWriter(new FileStream(path, FileMode.Append), Encoding.Default);
                sw.WriteLine(ip + " " + domain);

                //关闭写入
                if (sw != null)
                    sw.Close();

                File.SetAttributes(path, FileAttributes.ReadOnly);
            }
            catch (Exception ex)
            {

            }
        }
    }
}
