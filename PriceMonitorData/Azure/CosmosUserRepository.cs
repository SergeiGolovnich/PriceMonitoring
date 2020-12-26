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
using System.Data;

namespace PriceMonitorData.Azure
{
    public class CosmosUserRepository : UserRepository
    {
        private CosmosClient dbClient;

        private Container containerUsers;

        public CosmosUserRepository()
        {
            dbClient = new CosmosClient(EnvHelper.GetEnvironmentVariable("CosmosConnStr"));

            var db = dbClient.GetDatabase("PriceMonitorIdentity");
            containerUsers = db.GetContainer("Users");
        }
        public async Task<List<IdentityUser>> GetAllUsers(int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerUsers.GetItemLinqQueryable<IdentityUser>()
                .Where(u => u.PartitionKey == "IdentityUser")
                .OrderBy(x => x.Email)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .ToFeedIterator();

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
        public async Task<int> GetAllUsersTotalPagesAsync(int itemsPerPage = 10)
        {
            var itemesCountQuery = containerUsers.GetItemLinqQueryable<IdentityUser>()
                .Where(u => u.PartitionKey == "IdentityUser")
                .CountAsync();

            int itemsCount = (await itemesCountQuery).Resource;

            int pages = itemsCount / itemsPerPage;

            if (itemsCount % itemsPerPage != 0)
            {
                pages++;
            }

            return pages;
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

        public async Task<List<IdentityUser>> GetAllAdmins()
        {
            var setIterator = containerUsers.GetItemLinqQueryable<IdentityUser>().Where(u => u.PartitionKey == "IdentityUser" && u.FlattenRoleNames.Contains("Admin")).ToFeedIterator();

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

        public async Task<IdentityUser> RemoveUserFromRole(IdentityUser user, string roleName)
        {

            IdentityRole role = await GetRoleByName(roleName);

            IdentityUserRole identityUserRole = await GetIdentityUserRole(user, role);

            await DeleteIdentityUserRole(identityUserRole);

            List<string> roles = user.FlattenRoleNames.Split(',').Where(r => !string.IsNullOrEmpty(r) && r != role.Name).ToList();

            user.FlattenRoleNames = string.Join(',', roles);

            List<string> roleIds = user.FlattenRoleIds.Split(',').Where(r => !string.IsNullOrEmpty(r) && r != role.Id).ToList();

            user.FlattenRoleIds = string.Join(',', roleIds);

            user = await UpdateUser(user);

            return user;
        }
        public async Task<IdentityUser> UpdateUser(IdentityUser user)
        {
            var responce = await containerUsers.ReplaceItemAsync(user, user.Id, new PartitionKey(user.PartitionKey));

            return responce.Resource;
        }
        private async Task<IdentityRole> GetRoleByName(string roleName)
        {
            var setIterator = containerUsers.GetItemLinqQueryable<IdentityRole>().Where(r => r.PartitionKey == "IdentityRole" && r.NormalizedName == roleName.ToUpper()).Take(1).ToFeedIterator();

            IdentityRole output = null;

            while (setIterator.HasMoreResults)
            {
                foreach (var role in await setIterator.ReadNextAsync())
                {
                    output = role;
                }
            }

            if (output == null)
            {
                throw new Exception($"No role with name: {roleName}");
            }

            return output;
        }

        private async Task<IdentityUserRole> GetIdentityUserRole(IdentityUser user, IdentityRole role)
        {
            var setIterator = containerUsers.GetItemLinqQueryable<IdentityUserRole>()
                .Where(r => r.PartitionKey == "IdentityUserRole" && r.UserId == user.Id && r.RoleId == role.Id)
                .Take(1).ToFeedIterator();

            IdentityUserRole output = null;

            while (setIterator.HasMoreResults)
            {
                foreach (var userRole in await setIterator.ReadNextAsync())
                {
                    output = userRole;
                }
            }

            if (output == null)
            {
                throw new Exception($"No IdentityUserRole for UserId: {user.Id}, RoleId: {role.Id}.");
            }

            return output;
        }

        private async Task DeleteIdentityUserRole(IdentityUserRole identityUserRole)
        {
            ItemResponse<IdentityUserRole> Response = await containerUsers.DeleteItemAsync<IdentityUserRole>(identityUserRole.Id, new PartitionKey(identityUserRole.PartitionKey));
        }
    }
}
