using SendGrid;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData
{
    public class EmailSender
    {
        private SendGridClient client;

        public EmailSender()
        {
            string key = EnvHelper.GetEnvironmentVariable("SENDGRIDAPIKEY");

            client = new SendGridClient(key);
        }

        public async Task SendEmail()
        {

        }
    }
}
