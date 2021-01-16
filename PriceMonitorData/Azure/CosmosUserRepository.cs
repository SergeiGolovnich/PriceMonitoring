using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
//using IdentityUser = Mobsites.AspNetCore.Identity.Cosmos.IdentityUser;
//using IdentityRole = Mobsites.AspNetCore.Identity.Cosmos.IdentityRole;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using Mobsites.AspNetCore.Identity.Cosmos;
using System.Data;
using Microsoft.AspNetCore.Identity;

namespace PriceMonitorData.Azure
{
    public class CosmosUserRepository : UserRepository
    {
        private CosmosClient dbClient;

        private Container containerUsers;

        public CosmosUserRepository(string connectionString)
        {
            dbClient = new CosmosClient(connectionString);

            var db = dbClient.GetDatabase("PriceMonitorIdentity");
            containerUsers = db.GetContainer("Users");
        }
        public async Task<List<Microsoft.AspNetCore.Identity.IdentityUser>> GetAllUsersAsync(int page = 1, int itemsPerPage = 10)
        {
            var setIterator = containerUsers.GetItemLinqQueryable<Mobsites.AspNetCore.Identity.Cosmos.IdentityUser>()
                .Where(u => u.PartitionKey == "IdentityUser")
                .OrderBy(x => x.Email)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .ToFeedIterator();

            List<Microsoft.AspNetCore.Identity.IdentityUser> output = new List<Microsoft.AspNetCore.Identity.IdentityUser>();

            //Asynchronous query execution
            while (setIterator.HasMoreResults)
            {
                foreach (var user in await setIterator.ReadNextAsync())
                {
                    output.Add((Microsoft.AspNetCore.Identity.IdentityUser)user);
                }
            }

            return output;
        }
        public async Task<int> GetAllUsersTotalPagesAsync(int itemsPerPage = 10)
        {
            var itemesCountQuery = containerUsers.GetItemLinqQueryable<Mobsites.AspNetCore.Identity.Cosmos.IdentityUser>()
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
        public async Task<Microsoft.AspNetCore.Identity.IdentityUser> GetUserByIdAsync(string id)
        {
            QueryDefinition queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.id = \"" + id + "\"");

            FeedIterator<Mobsites.AspNetCore.Identity.Cosmos.IdentityUser> queryResultSetIterator = containerUsers.GetItemQueryIterator<Mobsites.AspNetCore.Identity.Cosmos.IdentityUser>(queryDefinition);

            Microsoft.AspNetCore.Identity.IdentityUser output = null;

            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Mobsites.AspNetCore.Identity.Cosmos.IdentityUser> currentResultSet = await queryResultSetIterator.ReadNextAsync();

                foreach (Microsoft.AspNetCore.Identity.IdentityUser user in currentResultSet)
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

        public async Task<List<Microsoft.AspNetCore.Identity.IdentityUser>> GetAllAdminsAsync()
        {
            var setIterator = containerUsers.GetItemLinqQueryable<Mobsites.AspNetCore.Identity.Cosmos.IdentityUser>().Where(u => u.PartitionKey == "IdentityUser" && u.FlattenRoleNames.Contains("Admin")).ToFeedIterator();

            List<Microsoft.AspNetCore.Identity.IdentityUser> output = new List<Microsoft.AspNetCore.Identity.IdentityUser>();

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

        public async Task<Microsoft.AspNetCore.Identity.IdentityUser> RemoveUserFromRoleAsync(Microsoft.AspNetCore.Identity.IdentityUser user, string roleName)
        {
            Microsoft.AspNetCore.Identity.IdentityRole role = await GetRoleByNameAsync(roleName);

            IdentityUserRole identityUserRole = await GetIdentityUserRoleAsync(user as Mobsites.AspNetCore.Identity.Cosmos.IdentityUser, role as Mobsites.AspNetCore.Identity.Cosmos.IdentityRole);

            await DeleteIdentityUserRoleAsync(identityUserRole);

            List<string> roles = (user as Mobsites.AspNetCore.Identity.Cosmos.IdentityUser).FlattenRoleNames.Split(',').Where(r => !string.IsNullOrEmpty(r) && r != role.Name).ToList();

            (user as Mobsites.AspNetCore.Identity.Cosmos.IdentityUser).FlattenRoleNames = string.Join(',', roles);

            List<string> roleIds = (user as Mobsites.AspNetCore.Identity.Cosmos.IdentityUser).FlattenRoleIds.Split(',').Where(r => !string.IsNullOrEmpty(r) && r != role.Id).ToList();

            (user as Mobsites.AspNetCore.Identity.Cosmos.IdentityUser).FlattenRoleIds = string.Join(',', roleIds);

            user = await UpdateUserAsync(user);

            return user;
        }
        public async Task<Microsoft.AspNetCore.Identity.IdentityUser> UpdateUserAsync(Microsoft.AspNetCore.Identity.IdentityUser user)
        {
            var responce = await containerUsers.ReplaceItemAsync(user, user.Id, new PartitionKey((user as Mobsites.AspNetCore.Identity.Cosmos.IdentityUser).PartitionKey));

            return responce.Resource;
        }
        private async Task<Microsoft.AspNetCore.Identity.IdentityRole> GetRoleByNameAsync(string roleName)
        {
            var setIterator = containerUsers.GetItemLinqQueryable<Mobsites.AspNetCore.Identity.Cosmos.IdentityRole>().Where(r => r.PartitionKey == "IdentityRole" && r.NormalizedName == roleName.ToUpper()).Take(1).ToFeedIterator();

            Microsoft.AspNetCore.Identity.IdentityRole output = null;

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

        private async Task<IdentityUserRole> GetIdentityUserRoleAsync(Mobsites.AspNetCore.Identity.Cosmos.IdentityUser user, Mobsites.AspNetCore.Identity.Cosmos.IdentityRole role)
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

        private async Task DeleteIdentityUserRoleAsync(Mobsites.AspNetCore.Identity.Cosmos.IdentityUserRole identityUserRole)
        {
            ItemResponse<IdentityUserRole> Response = await containerUsers.DeleteItemAsync<IdentityUserRole>(identityUserRole.Id, new PartitionKey(identityUserRole.PartitionKey));
        }
    }
}
