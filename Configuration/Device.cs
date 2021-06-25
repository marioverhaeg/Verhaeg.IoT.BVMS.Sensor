using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Verhaeg.IoT.Configuration
{
    public class Device: Generic
    {

        public Device(string path, string type): base(path, type)
        {

        }

        public System.Uri URI()
        {
            Log.Debug("Returning URI: " + conf.GetValue<string>(type + ":URI"));
            return new Uri(conf.GetValue<string>(type + ":URI"), UriKind.Absolute);
        }

        public string Username()
        {
            Log.Debug("Returning Username...");
            return conf.GetValue<string>(type + ":Username");
        }

        public string Password()
        {
            Log.Debug("Returning Password...");
            return conf.GetValue<string>(type + ":Password");
        }
    }
}
