using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bosch.Vms.SDK;
using Verhaeg.IoT.Ditto;

namespace Verhaeg.IoT.BVMS.Sensor.Managers
{
    public class EventManager : Processor.QueueManager
    {
        // SingleTon
        private static EventManager _instance = null;
        private static readonly object padlock = new object();

        // Configuration
        private Configuration.BVMS bvms_configuration;

        // Connection
        private RemoteServerApi rsa;
        private State.BVMS.Event em;

        private EventManager([System.Runtime.CompilerServices.CallerMemberName] string memberName = "") : base("EventManager")
        {
            Log.Debug("Starting EventManager from " + memberName);
            Log.Debug("Loading configuration.");
            bvms_configuration = Configuration.BVMS.Instance(System.AppDomain.CurrentDomain.BaseDirectory + "Configuration" + System.IO.Path.AltDirectorySeparatorChar + "BVMS.json");
            while (Connect() == false)
            {
                Log.Debug("Retrying to establish connection and subscribe to events.");
            }
        }

        private bool Connect()
        {
            Log.Debug("Trying to connect to BVMS server.");
            try
            {
                rsa = new RemoteServerApi(bvms_configuration.IP() + ":5390", bvms_configuration.username, bvms_configuration.password);
                rsa.ConnectionLostEvent += Rsa_ConnectionLostEvent;
                Log.Debug("Connection established.");
                if (Subscribe())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Could not connect to BVMS server, retry...");
                Log.Debug(ex.ToString());
                System.Threading.Thread.Sleep(5000);
                return false;
            }
        }

        private bool Subscribe()
        {
            Log.Debug("Trying to subscribe to BVMS events.");
            try
            {
                em = new State.BVMS.Event();
                rsa.EventManager.Register(em);
                Log.Debug("Subscription succeeded.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Could not register to EventReceiver.");
                Log.Debug(ex.ToString());
                return false;
            }
        }

        public void GetInitialStates()
        {
            Log.Debug("Trying to get initial states");
            rsa.DeviceManager.GetInitialStates();
        }

        public static EventManager Instance()
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new EventManager();
                }
                return (EventManager)_instance;
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
            rsa.EventManager.Unregister(em);
            rsa.Dispose();
        }

        protected override void Process(object obj)
        {
            EventData ed = (EventData)obj;
            Log.Debug("Received event from device: " + ed.DeviceName + " Type: " + ed.Type + " Old state: " + ed.State.Old + " New state: " + ed.State.New);
            UpdateDigitalTwin(ed);
        }

        private void UpdateDigitalTwin(EventData ed)
        {
            // Create Ditto thing
            NewThing t = new NewThing();
            t.Definition = "Verhaeg.IoT.BVMS:Event:1.0.0";

            // Create attributes
            Attributes ats = new Attributes();
            ats.AdditionalProperties.Add("name", bvms_configuration.Prefix() + "." + ed.Type + ":" + ed.DeviceName.Replace(" ",""));
            ats.AdditionalProperties.Add("type", ed.Type);

            // Create feature definition for relay status
            FeatureDefinition fd = new FeatureDefinition();
            fd.Add("Verhaeg.IoT.BVMS:Event:1.0.0");

            // Create feature properties for relay status
            FeatureProperties fp = new FeatureProperties();

            switch (ed.State.New)
            {
                case "Inactive":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                case "Active":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Off":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                case "None":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                case "On":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "VcaResultTrue":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "VcaResultFalse":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                case "Detected":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "NotDetected":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                case "Alarm":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "PreAlarm":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Connected":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Ok":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "SignalOk":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Unknown":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                default:
                    fp.AdditionalProperties.Add("state", 0);
                    Log.Error("State unknown: " + ed.State.New);
                    break;
            }
            
            fp.AdditionalProperties.Add("description", ed.State.New);          

            // Create feature for Relay status
            Feature f = new Feature();
            f.Definition = fd;
            f.Properties = fp;

            // Create list of features
            Features fs = new Features();
            fs.Add("Status", f);

            // Add features and attributes to thing
            t.Features = fs;
            t.Attributes = ats;

            Log.Debug("Sending thing to DittoManager...");

            // Send data to Ditto
            DittoManager.Instance().Write(t);
        }
    }
}
