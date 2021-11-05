using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bosch.Vms.SDK;
using Verhaeg.IoT.Ditto;

namespace Verhaeg.IoT.BVMS.Sensor.Managers
{
    public class AlarmManager : Processor.QueueManager
    {
        // SingleTon
        private static AlarmManager _instance = null;
        private static readonly object padlock = new object();

        // Configuration
        private Configuration.BVMS bvms_configuration;

        // Connection
        private RemoteServerApi rsa;
        private State.BVMS.Alarm am;

        private AlarmManager([System.Runtime.CompilerServices.CallerMemberName] string memberName = "") : base("AlarmManager")
        {
            Log.Debug("Starting AlarmManager from " + memberName);
            Log.Debug("Loading configuration.");
            bvms_configuration = Configuration.BVMS.Instance(System.AppDomain.CurrentDomain.BaseDirectory + "Configuration" + System.IO.Path.AltDirectorySeparatorChar + "BVMS.json");
            while (Connect() == false)
            {
                Log.Debug("Retrying to establish connection and subscribe to alarms.");
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
                return false;
            }
        }

        private bool Subscribe()
        {
            Log.Debug("Trying to subscribe to BVMS events.");
            try
            {
                am = new State.BVMS.Alarm();
                rsa.AlarmManager.Register(am);
                Log.Debug("Subscription succeeded.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Could not register to AlarmReceiver.");
                Log.Debug(ex.ToString());
                return false;
            }
        }

        public static AlarmManager Instance()
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new AlarmManager();
                }
                return (AlarmManager)_instance;
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
            rsa.AlarmManager.Unregister(am);
            rsa.Dispose();
        }

        protected override void Process(object obj)
        {
            AlarmData ad = (AlarmData)obj;
            Log.Debug("Received alarm from device: " + ad.Device + " Name: " + ad.DisplayName + " state: " + ad.State);
            UpdateDigitalTwin(ad);
        }

        private void UpdateDigitalTwin(AlarmData ad)
        {
            // Create Ditto thing
            NewThing t = new NewThing();
            t.Definition = "Verhaeg.IoT.BVMS:Alarm:1.0.0";

            Device d = rsa.DeviceManager.GetDeviceById(ad.Device.Id);

            // Create attributes
            Attributes ats = new Attributes();
            ats.AdditionalProperties.Add("name", bvms_configuration.Prefix() + ":" + ad.DisplayName.Replace(" ", ""));
            ats.AdditionalProperties.Add("device", rsa.DeviceManager.GetName(d).Replace(" ", ""));

            // Create feature definition for relay status
            FeatureDefinition fd = new FeatureDefinition();
            fd.Add("Verhaeg.IoT.BVMS:Alarm:1.0.0");

            // Create feature properties for relay status
            FeatureProperties fp = new FeatureProperties();

            switch (ad.State.ToString())
            {
                case "Created":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                case "Active":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Accepted":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Workflow":
                    fp.AdditionalProperties.Add("state", 1);
                    break;
                case "Cleared":
                    fp.AdditionalProperties.Add("state", 0);
                    break;
                default:
                    fp.AdditionalProperties.Add("state", 0);
                    Log.Error("State unknown: " + ad.State.ToString());
                    break;
            }
            
            fp.AdditionalProperties.Add("description", ad.State.ToString());
            fp.AdditionalProperties.Add("priority", ad.Priority);
            fp.AdditionalProperties.Add("title", ad.DisplayName);

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
