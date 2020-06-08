using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using PriceMonitorData;
using System.Collections.Generic;
using PriceMonitorSites;
using SimpleLRUCache;
using System.Runtime.CompilerServices;

namespace PriceMonitoring
{
    public static class CheckPrice
    {
#if DEBUG
        private const string rcon = "*/5 * * * * *";
#else
        private const string rcon = "0 0 6 * * *";
#endif
        private static ItemPriceRepository db;
        private static List<Item> items = new List<Item>();

        private static LUCache<string, AngleSharp.Dom.IDocument> cache = new LUCache<string, AngleSharp.Dom.IDocument>();

        private static EmailSender emailSender;

        private static ILogger log;

        private static string Errors = String.Empty;

        [FunctionName("CheckPrice")]
        public static async Task Run([TimerTrigger(rcon)]TimerInfo myTimer, ILogger logger)
        {
            log = logger;

            log.LogInformation($"C# Timer trigger function {nameof(CheckPrice)} executed at: {DateTime.Now}");

            try
            {
                await Initialize();
            }
            catch (Exception ex)
            {
                log.LogError($"Can't access DB or Email Service: {ex.Message}");

                await NotifyAdminsAboutError($"CheckPrice can't access DB or Email Service: {ex.Message}");

                return;
            }
            
            foreach(Item item in items)
            {
                await CheckItemPrice(item);
            }

            if(!string.IsNullOrEmpty(Errors))
            {
                await NotifyAdminsAboutError(Errors);
            }
        }

        private static async Task Initialize()
        {
            db = new ItemPriceRepository();
            items = await db.GetAllItems();

            emailSender = new EmailSender();
        }

        private static async Task CheckItemPrice(Item item)
        {
            if (item.SubscribersEmails.Length == 0)
            {
                await DeleteUnusedItem(item);
                
                return;
            }

            decimal currentPrice = 0;

            try
            {
                currentPrice = await PriceParser.Parse(item, cache);
            }
            catch (Exception ex)
            {
                log.LogError($"Can't parse url {item.Url}: {ex.Message}.");

                Errors += $"Can't parse url {item.Url}: {ex.Message}.<br>";

                return;
            }

            log.LogInformation($"Item {item.Name} price: {currentPrice} rub.");

            Price prevPrice;
            try
            {
                prevPrice = await db.GetLastItemPrice(item);
            }
            catch
            {
                try
                {
                    await db.CreateItemPrice(item, currentPrice);
                }catch(Exception ex)
                {
                    log.LogError($"Can't write item price to db: {ex.Message}.");
                }
                
                return;
            }

            if (currentPrice < prevPrice.ItemPrice)
            {
                await NotifyOfPriceReduction(item, currentPrice, prevPrice);
            }

            if (currentPrice == prevPrice.ItemPrice)
            {
                return;
            }

            Price newPrice;
            try
            {
                newPrice = await db.CreateItemPrice(item, currentPrice);
            }
            catch (Exception ex)
            {
                log.LogError($"Can't save item {item.Name} price {currentPrice} to db: {ex.Message}.");
            }
        }

        private static async Task NotifyOfPriceReduction(Item item, decimal currentPrice, Price prevPrice)
        {
            log.LogInformation($"Informing {item.SubscribersEmails.Length} user(s) about price decrease.");

            try
            {
                await emailSender.SendEmailAboutPriceDecrease(item, currentPrice, prevPrice.ItemPrice);
            }
            catch (Exception ex)
            {
                log.LogError($"Can't send Emails to subscribers: {ex.Message}");
            }
        }
        private static async Task NotifyAdminsAboutError(string error)
        {
            log.LogInformation($"Informing admins about error.");

            try
            {
                var userRepo = new UserRepository();

                await emailSender.SendEmailAboutError(await userRepo.GetAllAdmins(), error);
            }
            catch (Exception ex)
            {
                log.LogError($"Can't send Emails to Admins: {ex.Message}");
            }
        }

        private static async Task DeleteUnusedItem(Item item)
        {
            try
            {
                await db.DeleteItem(item);

                log.LogWarning($"Item {item.Name} had no subscribers. Item deleted.");
            }
            catch (Exception ex)
            {
                log.LogError($"Can't delete item {item.Name}: {ex.Message}");
            }
        }
    }
}
