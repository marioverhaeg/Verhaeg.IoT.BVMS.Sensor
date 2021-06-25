using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Verhaeg.IoT.Configuration
{
    public class BVMS : Device
    {

        // SingleTon
        private static BVMS _instance = null;

        public static BVMS Instance(string path, [CallerMemberName] string caller = "")
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new BVMS(path);
                }
                return (BVMS)_instance;
            }
        }


        private BVMS(string path) : base (path, "BVMS")
        {
            
        }

        public string IP()
        {
            Log.Debug("Returning IP: " + conf.GetValue<string>(type + ":IP"));
            return conf.GetValue<string>(type + ":IP");
        }

        public string Prefix()
        {
            Log.Debug("Returning Prefix: " + conf.GetValue<string>(type + ":Prefix"));
            return conf.GetValue<string>(type + ":Prefix");
        }


    }
}
