using Mobsites.AspNetCore.Identity.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.SQLite
{
    public class SQLiteUserRepository : UserRepository
    {
        public Task<List<IdentityUser>> GetAllAdminsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<IdentityUser>> GetAllUsersAsync(int page = 1, int itemsPerPage = 10)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAllUsersTotalPagesAsync(int itemsPerPage = 10)
        {
            throw new NotImplementedException();
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
