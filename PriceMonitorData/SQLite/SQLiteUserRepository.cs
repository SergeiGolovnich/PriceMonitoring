using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PriceMonitorData.SQLite
{
    public class SQLiteUserRepository : UserRepository
    {
        private readonly SQLiteIdentityContext dbContext;
        public SQLiteUserRepository(SQLiteIdentityContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public Task<List<IdentityUser>> GetAllAdminsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<IdentityUser>> GetAllUsersAsync(int page = 1, int itemsPerPage = 10)
        {
            List<IdentityUser> output = dbContext.Users
                .OrderBy(x => x.Email)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage).ToList();

            return Task.FromResult(output);
        }

        public Task<int> GetAllUsersTotalPagesAsync(int itemsPerPage = 10)
        {
            var itemsCount = dbContext.Users.Count();

            int pages = itemsCount / itemsPerPage;

            if (itemsCount % itemsPerPage != 0)
            {
                pages++;
            }

            return Task.FromResult(pages);
        }

        public Task<IdentityUser> GetUserByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> RemoveUserFromRoleAsync(IdentityUser user, string roleName)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> UpdateUserAsync(IdentityUser user)
        {
            throw new NotImplementedException();
        }
    }
}
