using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceMonitorData
{
    public interface UserRepository
    {
        Task<List<IdentityUser>> GetAllAdminsAsync();
        Task<List<IdentityUser>> GetAllUsersAsync(int page = 1, int itemsPerPage = 10);
        Task<int> GetAllUsersTotalPagesAsync(int itemsPerPage = 10);
        Task<IdentityUser> GetUserByIdAsync(string id);
        Task<IdentityUser> RemoveUserFromRoleAsync(IdentityUser user, string roleName);
        Task<IdentityUser> UpdateUserAsync(IdentityUser user);
    }
}