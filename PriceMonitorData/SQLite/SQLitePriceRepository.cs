using PriceMonitorData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.SQLite
{
    public class SQLitePriceRepository : PriceRepository
    {
        private SQLiteContext context;
        public SQLitePriceRepository()
        {
            context = new SQLiteContext();
        }
        public async Task<Price> CreateItemPriceAsync(Item item, decimal price)
        {
            var priceObj = new Price
            {
                Id = Guid.NewGuid().ToString(),
                ItemId = item.Id,
                ItemPrice = price,
                Date = DateTime.Now
            };

            context.Prices.Add(priceObj);

            await context.SaveChangesAsync();

            return priceObj;
        }

        public Task DeleteItemPricesAsync(Item item)
        {
            throw new NotImplementedException();
        }

        public Task<List<Price>> GetAllItemPricesAsync(Item item)
        {
            List<Price> prices = context.Prices.Where(p => p.ItemId == item.Id).ToList();

            return Task.FromResult(prices);
        }

        public Task<List<Price>> GetItemPricesAsync(Item item, int page = 1, int itemsPerPage = 10)
        {
            List<Price> prices = context.Prices
                .Where(p => p.ItemId == item.Id)
                .OrderByDescending(x => x.Date)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage).ToList();

            return Task.FromResult(prices);
        }

        public Task<Price> GetLastItemPriceAsync(Item item)
        {
            Price lastPrice = null;

            lastPrice = context.Prices.Where(p => p.ItemId == item.Id).OrderByDescending(x => x.Date).First();

            if (lastPrice == null)
            {
                throw new Exception($"There is no price for item: {item.Name}.");
            }

            return Task.FromResult(lastPrice);
        }

        public Task<List<Price>> GetLastItemPricesAsync(Item item, int maxCount = 1)
        {
            List<Price> lastPrices  = context.Prices
                .Where(p => p.ItemId == item.Id)
                .OrderByDescending(x => x.Date)
                .Take(maxCount).ToList();

            if (lastPrices.Count == 0)
            {
                throw new Exception($"There is no price for item: {item.Name}.");
            }

            return Task.FromResult(lastPrices);
        }
    }
}
