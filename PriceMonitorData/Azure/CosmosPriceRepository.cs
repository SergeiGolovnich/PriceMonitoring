using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using PriceMonitorData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.Azure
{
    public class CosmosPriceRepository : PriceRepository
    {
        private CosmosDBContext context;
        public CosmosPriceRepository(string connectionString)
        {
            context = new CosmosDBContext(connectionString);
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

            ItemResponse<Price> PriceResponse = await context.Prices.CreateItemAsync(priceObj, new PartitionKey(priceObj.ItemId));

            return PriceResponse.Resource;

        }

        public async Task<Price> GetLastItemPriceAsync(Item item)
        {
            Price lastPrice = null;

            var setIterator = context.Prices.GetItemLinqQueryable<Price>().Where(p => p.ItemId == item.Id).OrderByDescending(x => x.Date).Take(1).ToFeedIterator();

            if (setIterator.HasMoreResults)
            {
                var results = await setIterator.ReadNextAsync();

                lastPrice = results.First();
            }

            if (lastPrice == null)
            {
                throw new Exception($"There is no price for item: {item.Name}.");
            }

            return lastPrice;
        }
        public async Task<List<Price>> GetLastItemPricesAsync(Item item, int maxCount = 1)
        {
            List<Price> lastPrices = new List<Price>(maxCount);

            var setIterator = context.Prices.GetItemLinqQueryable<Price>()
                .Where(p => p.ItemId == item.Id)
                .OrderByDescending(x => x.Date)
                .Take(maxCount)
                .ToFeedIterator();

            if (setIterator.HasMoreResults)
            {
                foreach (var price in await setIterator.ReadNextAsync())
                {
                    lastPrices.Add(price);
                }
            }

            if (lastPrices.Count == 0)
            {
                throw new Exception($"There is no price for item: {item.Name}.");
            }

            return lastPrices;
        }
        public async Task<List<Price>> GetAllItemPricesAsync(Item item)
        {
            var setIterator = context.Prices.GetItemLinqQueryable<Price>().Where(p => p.ItemId == item.Id).ToFeedIterator();

            List<Price> Prices = new List<Price>();

            while (setIterator.HasMoreResults)
            {
                foreach (Price price in await setIterator.ReadNextAsync())
                {
                    Prices.Add(price);
                }
            }

            return Prices;
        }
        public async Task<List<Price>> GetItemPricesAsync(Item item, int page = 1, int itemsPerPage = 10)
        {
            var setIterator = context.Prices.GetItemLinqQueryable<Price>()
                .Where(p => p.ItemId == item.Id)
                .OrderByDescending(x => x.Date)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .ToFeedIterator();

            List<Price> Prices = new List<Price>();

            while (setIterator.HasMoreResults)
            {
                foreach (Price price in await setIterator.ReadNextAsync())
                {
                    Prices.Add(price);
                }
            }

            return Prices;
        }
        public async Task DeleteItemPricesAsync(Item item)
        {
            List<Price> prices = await GetAllItemPricesAsync(item);

            foreach (var price in prices)
            {
                await context.Prices.DeleteItemAsync<Price>(price.Id, new PartitionKey(item.Id));
            }
        }
    }
}
