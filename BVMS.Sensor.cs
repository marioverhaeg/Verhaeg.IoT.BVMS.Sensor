using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

using Serilog;

namespace Verhaeg.IoT.BVMS.Sensor
{
    public partial class Service : ServiceBase
    {
        private Serilog.ILogger Log;

        public Service()
        {
            Log = Processor.Log.CreateLog("Service");
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Log.Information("Starting EventManager.");
            Managers.EventManager.Instance().GetInitialStates();
            Log.Information("Starting AlarmManager.");
            Managers.AlarmManager.Instance();
        }

        protected override void OnStop()
        {
            Log.Information("Stopping BVMS.Sensor.");
            Managers.EventManager.Instance().Disconnect();
            Managers.EventManager.Instance().Stop();
            Managers.AlarmManager.Instance().Disconnect();
            Managers.AlarmManager.Instance().Stop();
        }
    }
}
