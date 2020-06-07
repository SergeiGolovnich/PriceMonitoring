using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PriceMonitorData;
using System.Collections.Generic;
using PriceMonitorSites;
using SimpleLRUCache;

namespace PriceMonitoring
{
    public static class CheckPrice
    {
#if DEBUG
        private const string rcon = "*/5 * * * * *";
#else
        private const string rcon = "0 0 6 * * *";
#endif

        [FunctionName("CheckPrice")]
        public static async Task Run([TimerTrigger(rcon)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            ItemPriceRepository db;
            List<Item> items = new List<Item>();

            var cache = new LUCache<string, AngleSharp.Dom.IDocument>();

            EmailSender emailSender;

            try
            {
                db = new ItemPriceRepository();
                items = await db.GetAllItems();

                emailSender = new EmailSender();
            }
            catch(Exception ex)
            {
                log.LogError($"Can't access db: {ex.Message}");

                return;
            }
            
            foreach(Item item in items)
            {
                if(item.SubscribersEmails.Length == 0)
                {
                    await db.DeleteItem(item);

                    continue;
                }

                decimal currentPrice = 0;

                try
                {
                    currentPrice = await PriceParser.Parse(item, cache);
                }
                catch(Exception ex)
                {
                    log.LogError($"Can't parse url {item.Url}: {ex.Message}.");

                    continue;
                }

                log.LogInformation($"Item {item.Name} price: {currentPrice} rub.");

                Price prevPrice;
                try
                {
                    prevPrice = await db.GetLastItemPrice(item);
                }
                catch
                {
                    await db.CreateItemPrice(item, currentPrice);

                    continue;
                }

                if (currentPrice < prevPrice.ItemPrice)
                {
                    log.LogInformation($"Informing {item.SubscribersEmails.Length} user(s) about price decrease.");

                    //Notify users of price reductions
                    try
                    {
                        await emailSender.SendEmailPriceDecrease(item, currentPrice, prevPrice.ItemPrice);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Emails were not sended: {ex.Message}");
                    }
                }
                
                if(currentPrice == prevPrice.ItemPrice)
                {
                    continue;
                }

                Price updatedPrice;
                try
                {
                    updatedPrice = await db.CreateItemPrice(item, currentPrice);
                }
                catch (Exception ex)
                {
                    log.LogError($"Can't save item {item.Name} price {currentPrice} to db: {ex.Message}.");
                }
            }
        } 
    }
}
