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
    public abstract class QueueManager : IDisposable
    {
        // Fields
        protected ConcurrentQueue<object> cq;
        protected Boolean blKeepRunning;
        protected EventWaitHandle ewh;
        protected Task t;
        protected CancellationTokenSource cts;

        // Logging
        protected Serilog.ILogger Log;

        protected QueueManager(string name)
        {
            // Serilog Configuration
            Log = Processor.Log.CreateLog(name);
            Log.Debug("Created new log: " + name);

            // Thread monitoring
            cq = new ConcurrentQueue<object>();
            blKeepRunning = true;
            ewh = new AutoResetEvent(false);

            // Start new read thread
            cts = new CancellationTokenSource();
            CancellationToken ct = cts.Token;
            t = Task.Factory.StartNew(() => Start(), ct);
        }

        public virtual void Dispose()
        {
            Stop();
        }

        private void Start()
        {
            while (blKeepRunning)
            {
                if (cq.Count > 0)
                {
                    // Read message from queue
                    Log.Debug("Reading message from queue...");
                    Read();
                }
                else
                {
                    // Wait for next command
                    ewh.WaitOne();
                }
            }
            Log.Information("Stopped working. Restarting Manager...");
        }

        private void Read()
        {
            object obj;
            cq.TryDequeue(out obj);
            Log.Debug("Removed message from queue.");
            Process(obj);
        }

        // Add data to queue
        public void Write(object obj)
        {
            cq.Enqueue(obj);
            Log.Debug("Pushed message into processing queue.");
            Log.Debug("Queuesize: " + cq.Count.ToString());

            // Read queue
            ewh.Set();

            // Check queue status
            if (t.IsCompleted || t.IsFaulted)
            {
                Log.Error("QueueManager has issues. Stopping QueueManager.");
                if (t.Exception != null)
                {
                    Log.Debug(t.Exception.ToString());
                }

                Log.Error("Restarting QueueManager...");
                CancellationToken ct = cts.Token;
                t = Task.Factory.StartNew(() => Start(), ct);
            }
        }

        public void Stop()
        {
            Log.Debug("Stop command received, checking queue...");
            // Wait until queue is empty
            while (cq.Count != 0)
            {
                Log.Debug("Queue not empty, waiting until queue is empty.");
                Task.Delay(2000);
            }
            cts.Cancel();
            Log.Debug("Queue empty, stopped working.");
        }

        protected abstract void Process(object obj);
    }

    
}
