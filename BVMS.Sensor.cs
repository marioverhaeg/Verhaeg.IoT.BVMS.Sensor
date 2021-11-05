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
        private Configuration.Ditto.WebSocket cdw;

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
            Log.Information("Retrieving Alarm states from Ditto.");
            cdw = Configuration.Ditto.WebSocket.Instance(
                System.AppDomain.CurrentDomain.BaseDirectory + "Configuration" + System.IO.Path.AltDirectorySeparatorChar + "Ditto_WS.json");
            State.Ditto.InputState.Instance(cdw);
        }

        protected override void OnStop()
        {
            Log.Information("Stopping BVMS.Sensor.");
            State.Ditto.InputState.Instance(cdw).Stop();
            Managers.EventManager.Instance().Disconnect();
            Managers.EventManager.Instance().Stop();
            Managers.AlarmManager.Instance().Disconnect();
            Managers.AlarmManager.Instance().Stop();
        }
    }
}
