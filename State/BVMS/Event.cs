using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Serilog;

using Bosch.Vms.SDK;

namespace Verhaeg.IoT.BVMS.Sensor.State.BVMS
{
    public class Event : EventReceiver
    {
        // device state table
        private readonly Dictionary<Guid, Dictionary<string, EventData.StateInfo>> allCurrentDeviceStates = new Dictionary<Guid, Dictionary<string, EventData.StateInfo>>();
        private readonly object syncStateTable = new object();

        // Logging
        private Serilog.ILogger Log;

        public Event()
        {
            Log = Processor.Log.CreateLog("Event");
        }

        public override void OnEvent(EventData ed)
        {
            Log.Debug("Received event from device: " + ed.DeviceName);
            Log.Debug("Type: " + ed.Type);
            Log.Debug("Old state: " + ed.State.Old);
            Log.Debug("New state: " + ed.State.New);
            Log.Debug("DateTime: " + ed.Time.ToString());

            if (ed.State.New != ed.State.Old)
            {

                if (ed.State.IsValid)
                {
                    lock (syncStateTable)
                    {
                        Dictionary<string, EventData.StateInfo> deviceStates;
                        if (!allCurrentDeviceStates.TryGetValue(ed.Device.Id, out deviceStates))
                        {
                            Log.Debug("State unknown in cache, adding new entry in cache.");
                            deviceStates = new Dictionary<string, EventData.StateInfo>();
                            allCurrentDeviceStates.Add(ed.Device.Id, deviceStates);
                        }

                        Log.Debug("Updating state in cache.");
                        deviceStates[ed.Type] = ed.State;

                        Log.Debug("Sending update to BVMSManager");
                        Managers.EventManager.Instance().Write(ed);
                        
                    }
                }
            }
            else
            {
                Log.Debug("State did not change for " + ed.DeviceName + " - " + ed.Type + ":" + ed.State.New);
            }
        }

        public string GetCurrentDeviceState(Guid deviceId, string eventType)
        {
            lock (syncStateTable)
            {
                Dictionary<string, EventData.StateInfo> deviceStates;
                if (allCurrentDeviceStates.TryGetValue(deviceId, out deviceStates))
                {
                    EventData.StateInfo stateInfo;
                    if (deviceStates.TryGetValue(eventType, out stateInfo))
                    {
                        return stateInfo.New;
                    }
                }
            }
            return String.Empty;
        }
    }
}
