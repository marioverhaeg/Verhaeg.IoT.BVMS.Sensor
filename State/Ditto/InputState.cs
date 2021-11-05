using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.WebSockets;

using Verhaeg.IoT.Ditto;

namespace Verhaeg.IoT.BVMS.Sensor.State.Ditto
{
    public class InputState : DittoWebSocketManager
    {
        // SingleTon
        private static InputState _instance = null;
        private static readonly object padlock = new object();

        public static InputState Instance(Configuration.Ditto.WebSocket cdw)
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new InputState(cdw);
                }
                return (InputState)_instance;
            }
        }

        public override void SendToManager(string str)
        {
            Log.Debug("Trying to parse Ditto JSON response into Ditto Thing...");
            DittoWebSocketResponse dws = Parse(str);

            if (dws != null)
            {
                Log.Debug("Routing message to VirtualInputManager.");
                Managers.VirtualInputManager.Instance().Write(dws);
            }
            else
            {
                Log.Error("Could not parse response from Ditto.");
            }
        }

        private InputState(Configuration.Ditto.WebSocket cdw) : base("START-SEND-EVENTS?namespaces=Verhaeg.IoT.BVMS.Alarm.InputState", "State_InputState",
            cdw.uri.ToString(), cdw.username, cdw.password)
        {

        }
    }
}
