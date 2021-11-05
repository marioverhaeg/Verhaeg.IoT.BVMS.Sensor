using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;

using Bosch.Vms.SDK;

namespace Verhaeg.IoT.BVMS.Sensor.State.BVMS
{
    public class Alarm : AlarmReceiver
    {
        // device state table
        private readonly Dictionary<Guid, Dictionary<string, EventData.StateInfo>> allCurrentDeviceStates = new Dictionary<Guid, Dictionary<string, EventData.StateInfo>>();
        private readonly object syncStateTable = new object();

        // Logging
        private Serilog.ILogger Log;

        public Alarm()
        {
            Log = Processor.Log.CreateLog("Alarm");
        }

        public override void OnAlarm(AlarmData ad)
        {
            Log.Debug("Device: " + ad.Device.Id);
            Log.Debug("DisplayName: " + ad.DisplayName);
            Log.Debug("Priority: " + ad.Priority);
            Log.Debug("State: " + ad.State);
            Log.Debug("Comment: " + ad.Comment);

            Managers.AlarmManager.Instance().Write(ad);
        }

    }
}
