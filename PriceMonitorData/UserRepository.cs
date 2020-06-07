using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using IdentityUser = Mobsites.AspNetCore.Identity.Cosmos.IdentityUser;
using IdentityRole = Mobsites.AspNetCore.Identity.Cosmos.IdentityRole;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using Mobsites.AspNetCore.Identity.Cosmos;

namespace PriceMonitorData
{
    public class UserRepository
    {
        private CosmosClient dbClient;

        private Container containerUsers;

        public UserRepository()
        {
            dbClient = new CosmosClient(EnvHelper.GetEnvironmentVariable("CosmosConnStr"));

            var db = dbClient.GetDatabase("PriceMonitorIdentity");
            containerUsers = db.GetContainer("Users");
        }
        public async Task<List<IdentityUser>> GetAllUsers()
        {
            var setIterator = containerUsers.GetItemLinqQueryable<IdentityUser>().Where(u => u.PartitionKey == "IdentityUser").ToFeedIterator();

            List<IdentityUser> output = new List<IdentityUser>();

            //Asynchronous query execution
            while (setIterator.HasMoreResults)
            {
                foreach (var user in await setIterator.ReadNextAsync())
                {
                    output.Add(user);
                }
            }

            return output;
        }
        public async Task<IdentityUser> GetUserById(string id)
        {
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = \"" + id + "\"");

            FeedIterator<IdentityUser> queryResultSetIterator = containerUsers.GetItemQueryIterator<IdentityUser>(queryDefinition);

            IdentityUser output = null;

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<IdentityUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                foreach (IdentityUser user in currentResultSet)
                {
                    output = user;
                }
            }

            if (output == null)
            {
                throw new Exception($"There is no user with id: {id}.");
            }

            return output;
        }
    }
}
