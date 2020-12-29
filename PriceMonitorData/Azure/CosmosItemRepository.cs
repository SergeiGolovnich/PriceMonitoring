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
        private CosmosClient dbClient;

        private Container containerItems;
        public CosmosItemRepository(string connectionString)
        {
            dbClient = new CosmosClient(connectionString);

            dbClient.CreateDatabaseIfNotExistsAsync("PriceMonitorDB");
            var db = dbClient.GetDatabase("PriceMonitorDB");

            db.CreateContainerIfNotExistsAsync("Items", "/Url");

            containerItems = db.GetContainer("Items");
        }
        public async Task<List<Item>> GetAllItemsAsync(int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerItems.GetItemLinqQueryable<Item>()
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
            var itemesCountQuery = containerItems.GetItemLinqQueryable<Item>()
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
            ItemResponse<Item> itemResponse = await containerItems.ReadItemAsync<Item>(item.Id, new PartitionKey(item.Url));

            return itemResponse.Resource;
        }
        public async Task<Item> GetItemByIdAsync(string id)
        {
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = \"" + id + "\"");

            FeedIterator<Item> queryResultSetIterator = containerItems.GetItemQueryIterator<Item>(queryDefinition);

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

            ItemResponse<Item> ItemResponse = await containerItems.CreateItemAsync(itemToCreate, new PartitionKey(itemToCreate.Url));

            return ItemResponse.Resource;
        }
        public async Task<Item> DeleteItemAsync(Item item)
        {
            ItemResponse<Item> ItemResponse = await containerItems.DeleteItemAsync<Item>(item.Id, new PartitionKey(item.Url));

            //await DeleteItemPrices(item);

            return ItemResponse.Resource;
        }
        public async Task<List<Item>> GetItemsBySubscriberAsync(string email, int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerItems.GetItemLinqQueryable<Item>()
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
            var itemesCountQuery = containerItems.GetItemLinqQueryable<Item>()
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

            ItemResponse<Item> response = await containerItems.ReplaceItemAsync(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

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

            ItemResponse<Item> response = await containerItems.ReplaceItemAsync(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

            return response.Resource;
        }
        public async Task<Item> SearchItemByNameAndUrlAsync(string name, string url)
        {
            var Iterator = containerItems.GetItemLinqQueryable<Item>().Where(i => i.Name == name && i.Url == url).Take(1).ToFeedIterator();

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
