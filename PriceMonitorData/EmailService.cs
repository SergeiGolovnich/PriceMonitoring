using Microsoft.AspNetCore.Identity;
using PriceMonitorData.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceMonitorData
{
    public interface EmailService
    {
        Task SendEmailAboutErrorAsync(List<IdentityUser> admins, string errorMessage);
        Task SendEmailAboutPriceDecreaseAsync(Item item, decimal currPrice, decimal prevPrice);
    }
}