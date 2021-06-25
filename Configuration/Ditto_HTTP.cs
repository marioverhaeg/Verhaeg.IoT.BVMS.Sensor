using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Verhaeg.IoT.Configuration
{
    public class Ditto_HTTP : Device
    {

        // SingleTon
        private static Ditto_HTTP _instance = null;

        public static Ditto_HTTP Instance(string path, [CallerMemberName] string caller = "")
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new Ditto_HTTP(path);
                }
                return (Ditto_HTTP)_instance;
            }
        }


        private Ditto_HTTP(string path) : base (path, "Ditto_HTTP")
        {
            
        }

       
    }
}
