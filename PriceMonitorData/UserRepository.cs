using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using IdentityUser = Mobsites.AspNetCore.Identity.Cosmos.IdentityUser;
using IdentityRole = Mobsites.AspNetCore.Identity.Cosmos.IdentityRole;

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
            var sqlQueryText = "SELECT * FROM c";

            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<IdentityUser> queryResultSetIterator = containerUsers.GetItemQueryIterator<IdentityUser>(queryDefinition);

            List<IdentityUser> output = new List<IdentityUser>();

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<IdentityUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (IdentityUser user in currentResultSet)
                {
                    output.Add(user);
                }
            }

            return output;
        }
    }
}
