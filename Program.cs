using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Verhaeg.IoT.BVMS.Sensor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            //while (!Debugger.IsAttached)
            //{
             //   System.Threading.Thread.Sleep(1000);
            //}

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
