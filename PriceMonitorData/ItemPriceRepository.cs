﻿using System;
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

        public ItemPriceRepository()
        {
            dbClient = new CosmosClient(EnvHelper.GetEnvironmentVariable("CosmosConnStr"));

            var db = dbClient.GetDatabase("PriceMonitorDB");
            containerPrices = db.GetContainer("Prices");
            containerItems = db.GetContainer("Items");
        }
        public async Task<List<Item>> GetAllItems(int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerItems.GetItemLinqQueryable<Item>()
               .OrderBy(x => x.Name)
               .Skip(itemsPerPage * (page - 1))
               .Take(itemsPerPage)
               .ToFeedIterator();

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
        public async Task<int> GetAllItemsTotalPages(int itemsPerPage = 10)
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

        public async Task<Price> CreateItemPrice(Item item, decimal price)
        {
            var priceObj = new Price
            {
                Id = Guid.NewGuid().ToString(),
                ItemId = item.Id,
                ItemPrice = price,
                Date = DateTime.Now
            };

            ItemResponse<Price> PriceResponse = await containerPrices.CreateItemAsync<Price>(priceObj, new PartitionKey(item.Id));

            return PriceResponse.Resource;

        }

        public async Task<Price> GetLastItemPrice(Item item)
        {
            Price lastPrice = null;

            var setIterator = containerPrices.GetItemLinqQueryable<Price>().Where(p => p.ItemId == item.Id).OrderByDescending(x => x.Date).Take(1).ToFeedIterator();

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
        public async Task<List<Price>> GetLastItemPrices(Item item, int maxCount = 1)
        {
            List<Price> lastPrices = new List<Price>(maxCount);

            var setIterator = containerPrices.GetItemLinqQueryable<Price>()
                .Where(p => p.ItemId == item.Id)
                .OrderByDescending(x => x.Date)
                .Take(maxCount)
                .ToFeedIterator();

            if (setIterator.HasMoreResults)
            {
                foreach(var price in await setIterator.ReadNextAsync())
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
        public async Task<List<Price>> GetAllItemPrices(Item item)
        {
            var setIterator = containerPrices.GetItemLinqQueryable<Price>().Where(p => p.ItemId == item.Id).ToFeedIterator();

            List<Price> Prices = new List<Price>();

            //Asynchronous query execution
            while (setIterator.HasMoreResults)
            {
                foreach (Price price in await setIterator.ReadNextAsync())
                {
                    Prices.Add(price);
                }
            }

            return Prices;
        }
        public async Task<List<Price>> GetItemPrices(Item item, int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerPrices.GetItemLinqQueryable<Price>()
                .Where(p => p.ItemId == item.Id)
                .OrderByDescending(x => x.Date)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .ToFeedIterator();

            List<Price> Prices = new List<Price>();

            //Asynchronous query execution
            while (setIterator.HasMoreResults)
            {
                foreach (Price price in await setIterator.ReadNextAsync())
                {
                    Prices.Add(price);
                }
            }

            return Prices;
        }
        public async Task<Item> GetItem(Item item)
        {
            ItemResponse<Item> itemResponse = await containerItems.ReadItemAsync<Item>(item.Id, new PartitionKey(item.Url));

            return itemResponse.Resource;
        }
        public async Task<Item> GetItemById(string id)
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
        public async Task<Item> CreateItem(string itemName, string url, string[] emails)
        {
            var itemToCreate = new Item
            {
                Id = Guid.NewGuid().ToString(),
                Name = itemName,
                Url = url,
                SubscribersEmails = emails
            };

            ItemResponse<Item> ItemResponse = await containerItems.CreateItemAsync<Item>(itemToCreate, new PartitionKey(itemToCreate.Url));

            return ItemResponse.Resource;
        }

        public async Task<Item> DeleteItem(Item item)
        {
            ItemResponse<Item> ItemResponse = await containerItems.DeleteItemAsync<Item>(item.Id, new PartitionKey(item.Url));

            await DeleteItemPrices(item);

            return ItemResponse.Resource;
        }

        private async Task DeleteItemPrices(Item item)
        {
            List<Price> prices = await GetAllItemPrices(item);

            foreach (var price in prices)
            {
                await containerPrices.DeleteItemAsync<Price>(price.Id, new PartitionKey(item.Id));
            }
        }

        public async Task<List<Item>> GetItemsBySubscriber(string email, int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerItems.GetItemLinqQueryable<Item>()
                .Where(i => i.SubscribersEmails.Contains(email))
                .OrderBy(x => x.Name)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .ToFeedIterator();

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
        public async Task<int> GetItemsBySubscriberTotalPages(string email, int itemsPerPage = 10)
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
        public async Task<Item> AddSubscriberToItem(Item item, string email)
        {
            Item itemInDb = await GetItem(item);

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
            var itemInDb = await GetItem(item);

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
                return await DeleteItem(itemInDb);
            }

            ItemResponse<Item> response = await containerItems.ReplaceItemAsync<Item>(itemInDb, itemInDb.Id, new PartitionKey(itemInDb.Url));

            return response.Resource;
        }
        public async Task<Item> SearchItemByNameAndUrl(string name, string url)
        {
            var Iterator = containerPrices.GetItemLinqQueryable<Item>().Where(i => i.Name == name && i.Url == url).TakeLast(1).ToFeedIterator();

            Item searchItem = null;
            //Asynchronous query execution
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
