using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace PriceMonitorData
{
    public class CosmosDB
    {
        private CosmosClient dbClient;

        private Container containerPrices;
        private Container containerItems;

        public CosmosDB()
        {
            string uri = EnvHelper.GetEnvironmentVariable("DBURI");
            string key = EnvHelper.GetEnvironmentVariable("DBKEY");

            dbClient = new CosmosClient(uri, key);

            var db = dbClient.GetDatabase("PriceMonitorDB");
            containerPrices = db.GetContainer("Prices");
            containerItems = db.GetContainer("Items");
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
        public async Task UpdateItemPrice(string itemName, decimal itemPrice)
        {
            var priceObj = new Price
            {
                Date = DateTime.Now,
                Id = itemName,
                ItemName = itemName,
                ItemPrice = itemPrice
            };

            ItemResponse<Price> priceResponse = await containerPrices.ReplaceItemAsync<Price>(priceObj, priceObj.Id, new PartitionKey(priceObj.ItemName));

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

    }
}
