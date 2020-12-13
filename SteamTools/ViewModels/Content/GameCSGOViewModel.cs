using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using SteamTools.Win32;
using System.Windows.Forms;
using MetroRadiance.Interop.Win32;
using System.Diagnostics;
using SteamTool.Core.Common;
using System.Runtime.InteropServices;
using SteamTools.Services;
using SteamTool.Core;
using System.IO;
using SteamTools.Properties;

namespace SteamTools.ViewModels
{
    public class GameCSGOViewModel : Livet.ViewModel
    {
        private readonly SteamToolService SteamTool = SteamToolCore.Instance.Get<SteamToolService>();

        public string SteamServicesPath => Path.Combine(SteamTool.SteamPath, @"bin\steamservice.exe");


        private string _CmdOut;
        public string CmdOut
        {
            get => this._CmdOut;
            set
            {
                if (this._CmdOut != value)
                {
                    this._CmdOut = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public void FixCsgoVacIssue()
        {
            SteamTool.KillSteamProcess();
            var fixpath = Path.Combine(App.Instance.LocalAppData.FullName, "vacfix.cmd");
            File.WriteAllText(fixpath, Resources.VACFIX_BAT, Encoding.Default);
            Task.Run(() =>
            {
                Process p = new Process();
                p.StartInfo.FileName = fixpath;//要执行的程序名称 
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.StandardOutputEncoding = Encoding.Default;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口 
                p.Start();//启动程序 
                p.StandardInput.AutoFlush = true;
                p.StartInfo.Verb = "runas";
                //向CMD窗口发送输入信息： 
                //p.StandardInput.WriteLine(@Resources.VACFIX_BAT);
                //获取CMD窗口的输出信息： 
                StreamReader standardOutput = p.StandardOutput;
                while (!standardOutput.EndOfStream)
                {
                    string text = standardOutput.ReadLine();
                    CmdOut += text + "\r\n";
                    //if (text.Contains("完毕"))
                    //    break;
                }
                p.Close();
                standardOutput.Close();
            });
        }
    }
}
