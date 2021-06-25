using System;
using Microsoft.Extensions.Configuration;
using Serilog;


namespace Verhaeg.IoT.Configuration
{
    public class Generic
    {
        protected IConfiguration conf;
        protected static readonly object padlock = new object();
        protected Serilog.ILogger Log;
        protected string path;
        protected string type;

        protected Generic (string path, string type)
        {
            // Serilog Configuration
            Log = Verhaeg.IoT.Processor.Log.CreateLog("Configuration_" + type);

            this.type = type;
            this.path = path;
            Load();
        }


        private void Load()
        {
            Log.Debug("Trying to load configuration from " + path + "...");
            try
            {
                ConfigurationBuilder cb = new ConfigurationBuilder();
                cb.SetBasePath(System.IO.Directory.GetCurrentDirectory());
                cb.AddJsonFile(path: path, optional: false, reloadOnChange: true);
                conf = cb.Build();
                Log.Information("Configuration loaded.");
            }
            catch (Exception ex)
            {
                Log.Error("Failed to load configuration from " + path + ".");
                Log.Fatal(ex.ToString());
            }
        }

        
    }
}
