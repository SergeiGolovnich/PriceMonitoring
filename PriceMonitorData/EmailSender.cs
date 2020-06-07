using SendGrid;
using SendGrid.Helpers.Mail;
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
        private EmailAddress from = new EmailAddress("noreply@pricemonitor.com", "Price Inform Bot");

        public EmailSender()
        {
            string key = EnvHelper.GetEnvironmentVariable("SENDGRIDAPIKEY");

            client = new SendGridClient(key);
        }

        public async Task SendEmailPriceDecrease(Item item, decimal currPrice, decimal prevPrice)
        {
            List<EmailAddress> tos = new List<EmailAddress>();
            foreach(string userEmail in item.SubscribersEmails)
            {
                tos.Add(new EmailAddress(userEmail));
            }

            decimal decrPerc = (prevPrice - currPrice) / prevPrice * 100m;
            var subject = $"{item.Name}'s price decreased by {decrPerc}%";

            var htmlContent = $"<strong>Item: </strong>{item.Name}<br>" +
                $"<strong>Price: </strong>{currPrice} rub<br>" +
                $"<strong>Link: </strong><a href='{item.Url}'>{item.Url}</a>";

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
            var response = await client.SendEmailAsync(msg);

            if(response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Can't send Email messages.");
            }
        }
    }
}
