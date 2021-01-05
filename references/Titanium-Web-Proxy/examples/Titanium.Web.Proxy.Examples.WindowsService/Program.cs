using System.ServiceProcess;

namespace WindowsServiceExample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ProxyService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
