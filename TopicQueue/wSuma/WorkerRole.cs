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

namespace wSuma
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("wSuma is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
                //Run1(this.cancellationTokenSource.Token);
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

            Trace.TraceInformation("wSuma has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("wSuma is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("wSuma has stopped");
        }

        SubscriptionClient m_client = SubscriptionClient.Create("obliczenia", "suma");
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
                    var msgResp = new BrokeredMessage(string.Format(" Suma elementów: " + arr.Sum(a => a)));
                    msgResp.SessionId = msg.ReplyToSessionId;
                    await m_queue.SendAsync(msgResp);
                    Trace.TraceInformation("wSuma: MessageId:{0}", msg.MessageId);
                    await msg.CompleteAsync();

                }
            }
        }


        /// <summary>
        /// Przykład jak zrobić coś podobnego ale przy użyciu wielu wątków - uwaga na "zatkanie" maszyny
        /// </summary>
        SubscriptionClient m_clientSuma = SubscriptionClient.Create("obliczenia", "suma");
        SubscriptionClient m_clientLiczba = SubscriptionClient.Create("obliczenia", "liczba");
        SubscriptionClient m_clientSrednia = SubscriptionClient.Create("obliczenia", "srednia");
        private void Run1(CancellationToken cancellationToken)
        {

            List<Task> lst = new List<Task>();
            lst.Add(Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var msg = await m_clientSuma.ReceiveAsync(TimeSpan.FromSeconds(60));
                    if (msg != null)
                    {
                        var arr = msg.GetBody<byte[]>();
                        var msgResp = new BrokeredMessage(string.Format(" Suma elementów: " + arr.Sum(a => a)));
                        msgResp.SessionId = msg.ReplyToSessionId;
                        await m_queue.SendAsync(msgResp);
                        Trace.TraceInformation("wSuma,Task: MessageId:{0}", msg.MessageId);
                        await msg.CompleteAsync();
                    }
                }
            }, cancellationToken)
                );
            lst.Add(Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var msg = await m_clientSrednia.ReceiveAsync(TimeSpan.FromSeconds(60));
                    if (msg != null)
                    {
                        var arr = msg.GetBody<byte[]>();
                        var msgResp = new BrokeredMessage(string.Format(" Średnia z elementów: " + arr.Average(a => a)));
                        msgResp.SessionId = msg.ReplyToSessionId;
                        await m_queue.SendAsync(msgResp);
                        Trace.TraceInformation("wSrednia,Task: MessageId:{0}", msg.MessageId);
                        await msg.CompleteAsync();
                    }
                }
            }, cancellationToken)
                );
            lst.Add(Task.Factory.StartNew(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var msg = await m_clientLiczba.ReceiveAsync(TimeSpan.FromSeconds(60));
                    if (msg != null)
                    {
                        var arr = msg.GetBody<byte[]>();
                        var msgResp = new BrokeredMessage(string.Format(" Liczba elementów: " + arr.Length));
                        msgResp.SessionId = msg.ReplyToSessionId;
                        await m_queue.SendAsync(msgResp);
                        Trace.TraceInformation("wLiczba,Task: MessageId:{0}", msg.MessageId);
                        await msg.CompleteAsync();
                    }
                }
            }, cancellationToken)
                );

            Task.WaitAll(lst.ToArray(), cancellationToken);
        }
    }
}
