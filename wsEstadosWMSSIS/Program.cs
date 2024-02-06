using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace wsEstadosWMSSIS
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        static void Main()
        {
            if (Environment.UserInteractive)
            {
                ServicioEstadosWmsSis service1 = new ServicioEstadosWmsSis();
                service1.TestStartupAndStop();
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new ServicioEstadosWmsSis()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
