using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

// A huge thanks to Stephen Toub for this:
// https://blogs.msdn.microsoft.com/pfxteam/2012/01/20/await-synchronizationcontext-and-console-apps/

namespace SlackDotNet.TestApp
{
    public static class EventLoop
    {
        public static void Run(Func<Task> func)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            var previousSyncContext = SynchronizationContext.Current;

            try
            {
                var syncContext = new SingleThreadSynchronizationContext();
                SynchronizationContext.SetSynchronizationContext(syncContext);

                var t = func();
                if (t == null)
                    throw new InvalidOperationException("No task provided.");

                t.ContinueWith(delegate { syncContext.Complete(); }, TaskScheduler.Default);

                syncContext.RunOnCurrentThread();

                t.GetAwaiter().GetResult();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousSyncContext);
            }
        }

        private sealed class SingleThreadSynchronizationContext : SynchronizationContext
        {
            private readonly BlockingCollection<(SendOrPostCallback, object)> queue = new BlockingCollection<(SendOrPostCallback, object)>();

            public override void Post(SendOrPostCallback d, object state)
            {
                if (d == null)
                    throw new ArgumentNullException(nameof(d));

                queue.Add((d, state));
            }

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("Synchronously sending is not supported.");
            }

            public void RunOnCurrentThread()
            {
                foreach ((SendOrPostCallback func, object state) workItem in queue.GetConsumingEnumerable())
                    workItem.func(workItem.state);
            }

            public void Complete()
            {
                queue.CompleteAdding();
            }
        }
    }
}
