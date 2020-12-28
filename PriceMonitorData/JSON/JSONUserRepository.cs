using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using IdentityUser = Mobsites.AspNetCore.Identity.Cosmos.IdentityUser;
using IdentityRole = Mobsites.AspNetCore.Identity.Cosmos.IdentityRole;

namespace PriceMonitorData.JSON
{
    class JSONUserRepository : UserRepository
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
