using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ServiceBusWebFrontend
{
    public partial class _Default : Page
    {
        TopicClient m_tc = TopicClient.Create("obliczenia");
        QueueClient m_qc = QueueClient.Create("wynik");
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected async void cmdSend1_Click(object sender, EventArgs e)
        {
            byte[] dataArr = {1,2,3,4,5,6,7,8,9,10};
            BrokeredMessage msg = new BrokeredMessage(dataArr);
            msg.Properties.Add("suma", 1);
            msg.Properties.Add("srednia", 1);
            msg.ReplyToSessionId = Guid.NewGuid().ToString("N");
            await m_tc.SendAsync(msg);
            MessageSession session=m_qc.AcceptMessageSession(msg.ReplyToSessionId, TimeSpan.FromSeconds(60));
            List<BrokeredMessage> lst = new List<BrokeredMessage>();
            while (lst.Count<3) 
			{
                msg = await session.ReceiveAsync();
                //Potem można: session.ReceiveAsync(msg.SequenceNumber + 1)
                if (msg != null)
                {
                    lst.Add(msg);
                    await msg.CompleteAsync();
                }
			 
			}
            lblInfo.Text = "";
            foreach (var item in lst) lblInfo.Text += item.GetBody<string>();
        }

        protected async void cmdSend2_Click(object sender, EventArgs e)
        {
            byte[] dataArr = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            BrokeredMessage msg = new BrokeredMessage(dataArr);
            msg.Properties.Add("suma", 1);
            msg.ReplyToSessionId = Guid.NewGuid().ToString("N");
            await m_tc.SendAsync(msg);
            MessageSession session = m_qc.AcceptMessageSession(msg.ReplyToSessionId, TimeSpan.FromSeconds(60));
            List<BrokeredMessage> lst = new List<BrokeredMessage>();
            while (lst.Count < 2)
            {
                msg = await session.ReceiveAsync();
                if (msg != null)
                {
                    lst.Add(msg);
                    await msg.CompleteAsync();
                }
            }
            lblInfo.Text = "";
            foreach (var item in lst) lblInfo.Text += item.GetBody<string>();
        }

    }
}