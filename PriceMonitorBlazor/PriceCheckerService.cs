using System;
using System.Threading.Tasks;
using System.Timers;
using PriceMonitorData;
using PriceMonitorData.Models;
using PriceMonitorSites;

namespace PriceMonitorBlazor
{
    public static class PriceCheckerService
    {
        private static Timer timer;
        private static ItemRepository itemRepo;
        private static PriceRepository priceRepo;

        private static string Errors;

        static PriceCheckerService()
        {
            timer = new Timer();
            timer.Elapsed += CheckPrices;
        }
        public static void Start(double interval, ItemRepository itemRepository, PriceRepository priceRepository)
        {
            itemRepo = itemRepository;
            priceRepo = priceRepository;

            timer.Interval = interval;
            timer.Start();
        }

        public static void CheckPrices(object sender, ElapsedEventArgs e)
        {
            var items = itemRepo.GetAllItemsAsync().Result;

            foreach (Item item in items)
            {
                CheckItemPrice(item).Wait();
            }

            if (!string.IsNullOrEmpty(Errors))
            {
                //await NotifyAdminsAboutError(Errors);
            }

            Errors = string.Empty;
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
                currentPrice = await PriceParser.Parse(item);
            }
            catch (Exception ex)
            {
                //log.LogError($"Can't parse url {item.Url}: {ex.Message}.");

                Errors += $"Can't parse url {item.Url}: {ex.Message}.<br>";

                return;
            }

            //log.LogInformation($"Item {item.Name} price: {currentPrice} rub.");

            Price prevPrice;
            try
            {
                prevPrice = await priceRepo.GetLastItemPriceAsync(item);
            }
            catch
            {
                try
                {
                    await priceRepo.CreateItemPriceAsync(item, currentPrice);
                }
                catch (Exception ex)
                {
                    //log.LogError($"Can't write item price to db: {ex.Message}.");
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
                newPrice = await priceRepo.CreateItemPriceAsync(item, currentPrice);
            }
            catch (Exception ex)
            {
                //log.LogError($"Can't save item {item.Name} price {currentPrice} to db: {ex.Message}.");
            }
        }
        private static async Task NotifyOfPriceReduction(Item item, decimal currentPrice, Price prevPrice)
        {
           //log.LogInformation($"Informing {item.SubscribersEmails.Length} user(s) about price decrease of {item.Name}.");

            try
            {
               // await emailSender.SendEmailAboutPriceDecreaseAsync(item, currentPrice, prevPrice.ItemPrice);
            }
            catch (Exception ex)
            {
                //log.LogError($"Can't send Emails to subscribers: {ex.Message}");
            }
        }
        private static async Task NotifyAdminsAboutError(string error)
        {
            //log.LogInformation($"Informing admins about Error.");

            try
            {
                //await emailSender.SendEmailAboutErrorAsync(await userRepo.GetAllAdmins(), error);
            }
            catch (Exception ex)
            {
                //log.LogError($"Can't send Emails to Admins: {ex.Message}");
            }
        }

        private static async Task DeleteUnusedItem(Item item)
        {
            try
            {
                await itemRepo.DeleteItemAsync(item);

                //log.LogWarning($"Item {item.Name} had no subscribers. Item deleted.");
            }
            catch (Exception ex)
            {
                //log.LogError($"Can't delete item {item.Name}: {ex.Message}");
            }
        }
    }
}
