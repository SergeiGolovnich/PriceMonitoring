using PriceMonitorData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using PriceMonitorSites;
using PriceMonitorData;
using Microsoft.Extensions.DependencyInjection;
using PriceMonitorData.SQLite;
using SimpleCache;
using SimpleLRUCache;
using AngleSharp.Dom;

namespace PriceMonitorBlazor.Services
{
    public class SQLitePriceCheckerService : PriceMonitorData.PriceCheckerService
    {
        private readonly IServiceProvider serviceProvider;
        private IServiceScope scope;
        private ItemRepository itemRepository;
        private PriceRepository priceRepository;

        private readonly Timer timer;
        private ICache<string, IDocument> cache = new LUCache<string, IDocument>(25);

        private List<string> errors = new List<string>();
        private DateTime lastCheckTime = DateTime.MinValue;

        public bool IsChecking { get; private set; }
        public SQLitePriceCheckerService(IServiceProvider serviceProvider)
        {
            timer = new Timer();
            timer.Interval = TimeSpan.FromDays(1).TotalMilliseconds;
            timer.Elapsed += Timer_Elapsed;
            this.serviceProvider = serviceProvider;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(CheckPricesAsync);
        }

        public DateTime LastCheckTime => lastCheckTime;

        public bool IsActive
        {
            get => timer.Enabled;
            set => timer.Enabled = value;
        }
        public TimeSpan Interval
        {
            get => TimeSpan.FromMilliseconds(timer.Interval);
            set => timer.Interval = value.TotalMilliseconds;
        }

        public IList<string> Errors => errors;

        public async Task CheckPricesAsync()
        {
            if (IsChecking) return;

            InitChecking();

            var items = itemRepository.GetAllItemsAsync().Result;

            foreach (Item item in items)
            {
                await CheckItemPrice(item);
            }

            if (errors.Count > 0)
            {
                //await NotifyAdminsAboutError(Errors);
            }

            CleanUpAfterChecking();
        }

        private void CleanUpAfterChecking()
        {
            itemRepository = null;
            priceRepository = null;
            scope.Dispose();
            scope = null;

            cache.Clear();

            IsChecking = false;
        }

        private void InitChecking()
        {
            scope = serviceProvider.CreateScope();

            priceRepository = scope.ServiceProvider.GetRequiredService<PriceRepository>();
            itemRepository = scope.ServiceProvider.GetRequiredService<ItemRepository>();
            
            errors.Clear();
            lastCheckTime = DateTime.Now;

            IsChecking = true;
        }

        private async Task CheckItemPrice(Item item)
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
                errors.Add($"Can't parse url {item.Url}: {ex.Message}.");

                return;
            }

            Price prevPrice;
            try
            {
                prevPrice = await priceRepository.GetLastItemPriceAsync(item);
            }
            catch
            {
                try
                {
                    await priceRepository.CreateItemPriceAsync(item, currentPrice);
                }
                catch (Exception ex)
                {
                    errors.Add($"Can't write item {item.Name} price {currentPrice} to db: {ex.Message}.");
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
                newPrice = await priceRepository.CreateItemPriceAsync(item, currentPrice);
            }
            catch (Exception ex)
            {
                errors.Add($"Can't write item {item.Name} price {currentPrice} to db: {ex.Message}.");
            }
        }
        private async Task NotifyOfPriceReduction(Item item, decimal currentPrice, Price prevPrice)
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
        private async Task NotifyAdminsAboutError(string error)
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
        private async Task DeleteUnusedItem(Item item)
        {
            try
            {
                await itemRepository.DeleteItemAsync(item);

                //log.LogWarning($"Item {item.Name} had no subscribers. Item deleted.");
            }
            catch (Exception ex)
            {
                errors.Add($"Can't delete item {item.Name}: {ex.Message}");
            }
        }
    }
}
