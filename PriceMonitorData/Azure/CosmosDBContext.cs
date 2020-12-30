using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.Azure
{
    class CosmosDBContext
    {
        public CosmosClient CosmosClient { get; private set; }
        public Container Items { get; private set; }
        public Container Prices { get; private set; }

        public CosmosDBContext(string connectionString)
        {
            CosmosClient = new CosmosClient(connectionString);

            CosmosClient.CreateDatabaseIfNotExistsAsync("PriceMonitorDB");
            var db = CosmosClient.GetDatabase("PriceMonitorDB");

            db.CreateContainerIfNotExistsAsync("Items", "/Url");
            db.CreateContainerIfNotExistsAsync("Prices", "/ItemId");

            Items = db.GetContainer("Items");
            Prices = db.GetContainer("Prices");
        }
    }
}
