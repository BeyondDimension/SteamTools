using System.Application.UI.Views;
using System.Windows.Forms;
using WinFormsApplication = System.Windows.Forms.Application;

namespace System
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            WinFormsApplication.SetHighDpiMode(HighDpiMode.SystemAware);
            WinFormsApplication.EnableVisualStyles();
            WinFormsApplication.SetCompatibleTextRenderingDefault(false);
            WinFormsApplication.Run(new Form1());
        }
    }
}