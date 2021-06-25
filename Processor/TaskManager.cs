using System;

// Extra
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Verhaeg.IoT.Processor
{
    public abstract class TaskManager : IDisposable
    {
        // Fields
        protected Boolean blKeepRunning;
        protected EventWaitHandle ewh;
        protected Task t;
        protected CancellationTokenSource cts;

        // Logging
        protected Serilog.ILogger Log;

        protected TaskManager(string name)
        {
            // Serilog Configuration
            Log = Processor.Log.CreateLog(name);
            Log.Debug("Created new log: " + name);

            blKeepRunning = true;
            ewh = new AutoResetEvent(false);
            // Start new read thread
            cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            t = Task.Factory.StartNew(() => Process(), ct);
        }

        public virtual void Dispose()
        {
            Stop();
        }

                
        public void Stop()
        {
            Log.Debug("Stop command received, checking queue...");
            // Wait until queue is empty
            cts.Cancel();
            Log.Debug("Stopped working.");
        }

        protected abstract void Process();
    }

    
}
