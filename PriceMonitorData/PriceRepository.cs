using PriceMonitorData.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceMonitorData
{
    public interface PriceRepository
    {
        Task<Price> CreateItemPriceAsync(Item item, decimal price);
        Task<List<Price>> GetAllItemPricesAsync(Item item);
        Task<List<Price>> GetItemPricesAsync(Item item, int page = 1, int itemsPerPage = 10);
        Task<Price> GetLastItemPriceAsync(Item item);
        Task<List<Price>> GetLastItemPricesAsync(Item item, int maxCount = 1);
    }
}