using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Verhaeg.IoT.Configuration.Ditto
{
    public class HTTP : Device
    {

        // SingleTon
        private static HTTP _instance = null;

        public static HTTP Instance(string path, [CallerMemberName] string caller = "")
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new HTTP(path);
                }
                return (HTTP)_instance;
            }
        }


        private HTTP(string path) : base (path, "Ditto_HTTP")
        {
            
        }

       
    }
}
