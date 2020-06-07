using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace PriceMonitorData
{
    public class ItemPriceRepository
    {
        private CosmosClient dbClient;

        private Container containerPrices;
        private Container containerItems;
        private Container containerUsers;

        public ItemPriceRepository()
        {
            dbClient = new CosmosClient(EnvHelper.GetEnvironmentVariable("CosmosConnStr"));

            var db = dbClient.GetDatabase("PriceMonitorDB");
            containerPrices = db.GetContainer("Prices");
            containerItems = db.GetContainer("Items");
            containerUsers = db.GetContainer("Users");
        }
        public async Task<List<Item>> GetAllItems()
        {
            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<Item> queryResultSetIterator = containerItems.GetItemQueryIterator<Item>(queryDefinition);

            List<Item> output = new List<Item>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Item> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Item item in currentResultSet)
                {
                    output.Add(item);
                }
            }

            return output;
        }
        public async Task<List<User>> GetAllUsers()
        {
            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<User> queryResultSetIterator = containerUsers.GetItemQueryIterator<User>(queryDefinition);

            List<User> output = new List<User>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<User> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (User user in currentResultSet)
                {
                    output.Add(user);
                }
            }

            return output;
        }
        public async Task<Price> UpdateItemPrice(string itemName, decimal itemPrice)
        {
            var priceObj = new Price
            {
                Date = DateTime.Now,
                Id = itemName,
                ItemName = itemName,
                ItemPrice = itemPrice
            };

            ItemResponse<Price> priceResponse = await containerPrices.ReplaceItemAsync<Price>(priceObj, priceObj.Id, new PartitionKey(priceObj.ItemName));

            return priceResponse.Resource;
        }

        public async Task<Price> CreateItemPrice(string itemName, decimal itemPrice)
        {
            var priceObj = new Price
            {
                Date = DateTime.Now,
                Id = itemName,
                ItemName = itemName,
                ItemPrice = itemPrice
            };

            try
            {
                ItemResponse<Price> PriceResponse = await containerPrices.CreateItemAsync<Price>(priceObj, new PartitionKey(priceObj.ItemName));

                return PriceResponse.Resource;
            }
            catch
            {
                return priceObj;
            }
        }

        public async Task<Price> GetItemPrice(string itemName)
        {
            ItemResponse<Price> PriceResponse = await containerPrices.ReadItemAsync<Price>(itemName, new PartitionKey(itemName));

            return PriceResponse.Resource;
        }
        public async Task<Item> GetItem(string itemName, string url)
        {
            ItemResponse<Item> itemResponse = await containerItems.ReadItemAsync<Item>(itemName, new PartitionKey(url));

            return itemResponse.Resource;
        }
        public async Task<Item> CreateItem(string itemName, string url)
        {
            var itemToCreate = new Item
            {
                Id = itemName,
                Name = itemName,
                Url = url
            };

            try
            {
                Item itemExists = await GetItem(itemName, url);

                return itemExists;
            }
            catch { }

            ItemResponse<Item> ItemResponse = await containerItems.CreateItemAsync<Item>(itemToCreate, new PartitionKey(itemToCreate.Url));

            return ItemResponse.Resource;
        }
        public async Task<Item> CreateItem(Item item)
        {
            ItemResponse<Item> ItemResponse = await containerItems.CreateItemAsync<Item>(item, new PartitionKey(item.Url));

            return ItemResponse.Resource;
        }
        public async Task<Item> DeleteItem(string itemName, string url)
        {
            Item itemObj = null;

            try
            {
                ItemResponse<Item> ItemResponse = await containerItems.DeleteItemAsync<Item>(itemName, new PartitionKey(url));

                return ItemResponse.Resource;
            }
            catch
            {
                return itemObj;
            }
        }
        public async Task<List<Item>> GetItemsBySubscriber(string email)
        {
            var setIterator = containerItems.GetItemLinqQueryable<Item>().Where(i => i.SubscribersEmails.Contains(email)).ToFeedIterator();

            List<Item> output = new List<Item>();

            //Asynchronous query execution
            while (setIterator.HasMoreResults)
            {
                foreach (var item in await setIterator.ReadNextAsync())
                {
                    output.Add(item);
                }
            }

            return output;
        }
        public async Task<Item> AddSubscriberToItem(Item item, string email)
        {
            Item itemInDb = await GetItem(item.Name, item.Url);

            if (itemInDb.SubscribersEmails.Contains(email))
            {
                return itemInDb;
            }

            itemInDb.SubscribersEmails = itemInDb.SubscribersEmails.Append(email).ToArray();

            ItemResponse<Item> response = await containerItems.ReplaceItemAsync<Item>(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

            return response.Resource;
        }
        public async Task<Item> RemoveSubscriberFromItem(Item item, string email)
        {
            var itemInDb = await GetItem(item.Name, item.Url);

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

            ItemResponse<Item> response = await containerItems.ReplaceItemAsync<Item>(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

            return response.Resource;
        }
    }
}
