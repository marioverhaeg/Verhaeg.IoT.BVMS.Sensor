using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bosch.Vms.SDK;
using Verhaeg.IoT.Ditto;

namespace Verhaeg.IoT.BVMS.Sensor.Managers
{
    public class VirtualInputManager : Processor.QueueManager
    {
        // SingleTon
        private static VirtualInputManager _instance = null;
        private static readonly object padlock = new object();

        // Configuration
        private Configuration.BVMS bvms_configuration;

        // Connection
        private RemoteServerApi rsa;

        private VirtualInputManager([System.Runtime.CompilerServices.CallerMemberName] string memberName = "") : base("VirtualInputManager")
        {
            Log.Debug("Starting VirtualInputManager from " + memberName);
            Log.Debug("Loading configuration.");
            bvms_configuration = Configuration.BVMS.Instance(System.AppDomain.CurrentDomain.BaseDirectory + "Configuration" + System.IO.Path.AltDirectorySeparatorChar + "BVMS.json");
            Connect();
        }

        private bool Connect()
        {
            Log.Debug("Trying to connect to BVMS server.");
            try
            {
                rsa = new RemoteServerApi(bvms_configuration.IP() + ":5390", bvms_configuration.username, bvms_configuration.password);
                rsa.ConnectionLostEvent += Rsa_ConnectionLostEvent;
                Log.Debug("Connection established.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Could not connect to BVMS server, retry...");
                Log.Debug(ex.ToString());
                return false;
            }
        }

        

        public static VirtualInputManager Instance()
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new VirtualInputManager();
                }
                return (VirtualInputManager)_instance;
            }
        }

        private void Rsa_ConnectionLostEvent(object sender, EventArgs e)
        {
            Log.Debug("Lost connection to BVMS management server, trying to restart.");
            while (Connect() == false)
            {
                Log.Debug("Retrying to establish connection and subscribe to events.");
            }
        }

        public void Disconnect()
        {
            rsa.Dispose();
        }

        protected override void Process(object obj)
        {
            DittoWebSocketResponse dwsr = (DittoWebSocketResponse)obj;
            Configuration.Ditto.Thing vi = new Configuration.Ditto.Thing(dwsr.value.ThingId);
            long new_state = (long)dwsr.value.Features["Status"].Properties.AdditionalProperties["state"];
            long current_state = GetVirtualInputState(vi.name);

            if (new_state != current_state)
            {
                Log.Debug("New state differs from current state, changing state in BVMS.");
                UpdateVirtualInputState(vi.name, new_state);
            }
        }

        private void UpdateVirtualInputState(string name, long state)
        {
            try
            {
                VirtualInput vi = rsa.VirtualInputManager.GetVirtualInputByName(name);
                if (state == 1)
                {
                    Log.Debug("Switching VirtualInput on: " + name);
                    rsa.VirtualInputManager.SwitchOn(vi);
                }
                else
                {
                    Log.Debug("Switching VirtualInput off: " + name);
                    rsa.VirtualInputManager.SwitchOff(vi);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not update VirtualInput state in BVMS.");
                Log.Debug(ex.ToString());
            }
        }

        private long GetVirtualInputState(string name)
        {
            try
            {
                VirtualInput vi = rsa.VirtualInputManager.GetVirtualInputByName(name);
                InputState ist = rsa.VirtualInputManager.GetState(vi);
                if (ist is InputState.On)
                {
                    return 1;
                }
                else if (ist is InputState.Off)
                {
                    return 0;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not retrieve VirtualInput state from BVMS.");
                Log.Debug(ex.ToString());
                return 0;
            }
        }

      
    }
}
