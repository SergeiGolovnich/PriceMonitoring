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
    public class CosmosItemRepository : ItemRepository
    {
        private CosmosDBContext context;
        public CosmosItemRepository(string connectionString)
        {
            context = new CosmosDBContext(connectionString);
        }
        public async Task<List<Item>> GetAllItemsAsync(int page = 1, int itemsPerPage = 10)
        {
            var setIterator = context.Items.GetItemLinqQueryable<Item>()
               .OrderBy(x => x.Name)
               .Skip(itemsPerPage * (page - 1))
               .Take(itemsPerPage)
               .ToFeedIterator();

            List<Item> output = new List<Item>();

            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync())
                {
                    output.Add(item);
                }
            }

            return output;
        }
        public async Task<int> GetAllItemsTotalPagesAsync(int itemsPerPage = 10)
        {
            var itemesCountQuery = context.Items.GetItemLinqQueryable<Item>()
                .CountAsync();

            int itemsCount = (await itemesCountQuery).Resource;

            int pages = itemsCount / itemsPerPage;

            if (itemsCount % itemsPerPage != 0)
            {
                pages++;
            }

            return pages;
        }
        public async Task<Item> GetItemAsync(Item item)
        {
            ItemResponse<Item> itemResponse = await context.Items.ReadItemAsync<Item>(item.Id, new PartitionKey(item.Url));

            return itemResponse.Resource;
        }
        public async Task<Item> GetItemByIdAsync(string id)
        {
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = \"" + id + "\"");

            FeedIterator<Item> queryResultSetIterator = context.Items.GetItemQueryIterator<Item>(queryDefinition);

            Item output = null;

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Item> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                foreach (Item item in currentResultSet)
                {
                    output = item;

                    break;
                }
            }

            if (output == null)
            {
                throw new Exception($"There is no item with id: {id}.");
            }

            return output;
        }
        public async Task<Item> CreateItemAsync(string itemName, string url, string[] emails)
        {
            var itemToCreate = new Item
            {
                Id = Guid.NewGuid().ToString(),
                Name = itemName,
                Url = url,
                SubscribersEmails = emails
            };

            ItemResponse<Item> ItemResponse = await context.Items.CreateItemAsync(itemToCreate, new PartitionKey(itemToCreate.Url));

            return ItemResponse.Resource;
        }
        public async Task<Item> DeleteItemAsync(Item item)
        {
            ItemResponse<Item> ItemResponse = await context.Items.DeleteItemAsync<Item>(item.Id, new PartitionKey(item.Url));

            await DeleteItemPrices(item);

            return ItemResponse.Resource;
        }
        private async Task DeleteItemPrices(Item item)
        {
            List<Price> prices = await GetAllItemPricesAsync(item);
            
            foreach (var price in prices)
            {
                await context.Prices.DeleteItemAsync<Price>(price.Id, new PartitionKey(item.Id));
            }
        }
        private async Task<List<Price>> GetAllItemPricesAsync(Item item)
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

        public async Task<List<Item>> GetItemsBySubscriberAsync(string email, int page = 1, int itemsPerPage = 10)
        {
            var setIterator = context.Items.GetItemLinqQueryable<Item>()
                .Where(i => i.SubscribersEmails.Contains(email))
                .OrderBy(x => x.Name)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .ToFeedIterator();

            List<Item> output = new List<Item>();

            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync())
                {
                    output.Add(item);
                }
            }

            return output;
        }
        public async Task<int> GetItemsBySubscriberTotalPagesAsync(string email, int itemsPerPage = 10)
        {
            var itemesCountQuery = context.Items.GetItemLinqQueryable<Item>()
                .Where(i => i.SubscribersEmails.Contains(email))
                .CountAsync();

            int itemsCount = (await itemesCountQuery).Resource;

            int pages = itemsCount / itemsPerPage;

            if (itemsCount % itemsPerPage != 0)
            {
                pages++;
            }

            return pages;
        }
        public async Task<Item> AddSubscriberToItemAsync(Item item, string email)
        {
            Item itemInDb = await GetItemAsync(item);

            if (itemInDb.SubscribersEmails.Contains(email))
            {
                return itemInDb;
            }

            itemInDb.SubscribersEmails = itemInDb.SubscribersEmails.Append(email).ToArray();

            ItemResponse<Item> response = await context.Items.ReplaceItemAsync(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

            return response.Resource;
        }
        public async Task<Item> RemoveSubscriberFromItemAsync(Item item, string email)
        {
            var itemInDb = await GetItemAsync(item);

            if (!itemInDb.SubscribersEmails.Contains(email))
            {
                return itemInDb;
            }

            if (itemInDb.SubscribersEmails.Contains(email))
            {
                var list = itemInDb.SubscribersEmails.ToList();

                list.Remove(email);

                itemInDb.SubscribersEmails = list.ToArray();
            }

            if (itemInDb.SubscribersEmails.Length == 0)
            {
                return await DeleteItemAsync(itemInDb);
            }

            ItemResponse<Item> response = await context.Items.ReplaceItemAsync(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

            return response.Resource;
        }
        public async Task<Item> SearchItemByNameAndUrlAsync(string name, string url)
        {
            var Iterator = context.Items.GetItemLinqQueryable<Item>().Where(i => i.Name == name && i.Url == url).Take(1).ToFeedIterator();

            Item searchItem = null;

            while (Iterator.HasMoreResults)
            {
                foreach (Item item in await Iterator.ReadNextAsync())
                {
                    searchItem = item;
                }
            }

            if (searchItem == null)
            {
                throw new Exception($"There is no item with name: {name}, url: {url}.");
            }

            return searchItem;
        }
    }
}
