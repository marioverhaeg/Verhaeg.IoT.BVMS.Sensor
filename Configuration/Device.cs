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
        public System.Uri uri
        {
            get
            {
                Log.Debug("Returning URI: " + conf.GetValue<string>(type + ":URI"));
                return new Uri(conf.GetValue<string>(type + ":URI"), UriKind.Absolute);
            }
        }

        public string username
        {
            get
            {
                Log.Debug("Returning Username...");
                return conf.GetValue<string>(type + ":Username");
            }
        }

        public string password
        {
            get
            {
                Log.Debug("Returning Password...");
                return conf.GetValue<string>(type + ":Password");
            }
        }
    }
}
