using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.ServiceBus.Messaging;

namespace wLiczba
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("wLiczba is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("wLiczba has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("wLiczba is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("wLiczba has stopped");
        }

        SubscriptionClient m_client = SubscriptionClient.Create("obliczenia", "liczba");
        QueueClient m_queue = QueueClient.Create("wynik");
        private async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following with your own logic.
            while (!cancellationToken.IsCancellationRequested)
            {
                var msg = await m_client.ReceiveAsync(TimeSpan.FromSeconds(60));
                if (msg != null)
                {
                    var arr = msg.GetBody<byte[]>();
                    var msgResp = new BrokeredMessage(string.Format(" Liczba element�w: " + arr.Length));
                    msgResp.SessionId = msg.ReplyToSessionId;
                    await m_queue.SendAsync(msgResp);
                    Trace.TraceInformation("wLiczba: MessageId:{0}", msg.MessageId);
                    await msg.CompleteAsync();
                }
            }
        }
    }
}
