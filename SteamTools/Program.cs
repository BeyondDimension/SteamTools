using SteamTool.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteamTools
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Logger.InitLogger($"{Assembly.GetExecutingAssembly().GetName().Name}ExceptionLogger", @"Log\log4net.config");
            App app = new App();
            app.InitializeComponent();
            try
            {
                app.Run();
            }
            catch (Exception ex)
            {
                Logger.Error($"{Assembly.GetExecutingAssembly().GetName().Name} Run Error : {Environment.NewLine}", ex);
                MessageBox.Show(ex.ToString(), "发生错误");
                throw ex;
            }
        }

    }
}
