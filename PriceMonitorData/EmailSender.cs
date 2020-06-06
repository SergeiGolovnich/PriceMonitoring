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

        public async Task SendEmailPriceDecrease(List<User> toUsers, Item item, decimal currPrice, decimal prevPrice)
        {
            List<EmailAddress> tos = new List<EmailAddress>();
            foreach(User user in toUsers)
            {
                tos.Add(new EmailAddress(user.Email, user.Name));
            }

            decimal decrPerc = (prevPrice - currPrice) / prevPrice * 100m;
            var subject = $"{item.Name}'s price decreased by {decrPerc}%";

            var htmlContent = $"<strong>Item: </strong>{item.Name}<br>" +
                $"<strong>Price: </strong>{currPrice} rub<br>" +
                $"<strong>Link: </strong><a href='{item.Url}'>{item.Url}</a>";

            var msg = MailHelper.CreateSingleEmailToMultipleRecipients(from, tos, subject, "", htmlContent, false);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
