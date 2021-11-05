using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Verhaeg.IoT.Configuration.Ditto
{
    public class WebSocket : Device
    {

        // SingleTon
        private static WebSocket _instance = null;

        public static WebSocket Instance(string path, [CallerMemberName] string caller = "")
        {
            lock (padlock)
            {
                if (_instance == null)
                {
                    _instance = new WebSocket(path);
                }
                return (WebSocket)_instance;
            }
        }


        private WebSocket(string path) : base (path, "Ditto_WS")
        {
            
        }

       
    }
}
