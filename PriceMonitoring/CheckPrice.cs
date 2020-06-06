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

            CosmosDB db;
            List<Item> items = new List<Item>();

            var cache = new LUCache<string, >

            EmailSender emailSender;
            List<User> users = new List<User>();
            try
            {
                db = new CosmosDB();
                items = await db.GetAllItems();

                emailSender = new EmailSender();
                users = await db.GetAllUsers();
            }
            catch(Exception ex)
            {
                log.LogInformation($"Can't access db: {ex.Message}");

                return;
            }
            
            foreach(Item item in items)
            {
                decimal currentPrice = 0;

                try
                {
                    currentPrice = await PriceParser.Parse(item.Url, item.Name);
                }
                catch(Exception ex)
                {
                    log.LogInformation($"Can't parse url {item.Url}: {ex.Message}.");

                    continue;
                }

                log.LogInformation($"Item {item.Name} price: {currentPrice} rub.");

                Price prevPrice;
                try
                {
                    prevPrice = await db.GetItemPrice(item.Name);
                }
                catch
                {
                    await db.CreateItemPrice(item.Id, currentPrice);

                    continue;
                }

                Price updatedPrice;
                try
                {
                    updatedPrice = await db.UpdateItemPrice(item.Id, currentPrice);
                }
                catch (Exception ex)
                {
                    log.LogInformation($"Can't save item {item.Name} price {currentPrice} to db: {ex.Message}.");

                    continue;
                }

                if (currentPrice < prevPrice.ItemPrice)
                {
                    log.LogInformation($"Informing {users.Count} user(s) about price decrease.");

                    //Notify users of price reductions
                    await emailSender.SendEmailPriceDecrease(users, item, updatedPrice, prevPrice.ItemPrice);
                }
            }
        } 
    }
}
