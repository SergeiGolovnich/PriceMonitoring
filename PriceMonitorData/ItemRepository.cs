using PriceMonitorData.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PriceMonitorData
{
    public interface ItemRepository
    {
        Task<Item> AddSubscriberToItemAsync(Item item, string email);
        Task<Item> CreateItemAsync(string itemName, string url, string[] emails);
        Task<Item> DeleteItemAsync(Item item);
        Task<List<Item>> GetAllItemsAsync(int page = 1, int itemsPerPage = 10);
        Task<int> GetAllItemsTotalPagesAsync(int itemsPerPage = 10);
        Task<Item> GetItemAsync(Item item);
        Task<Item> GetItemByIdAsync(string id);
        Task<List<Item>> GetItemsBySubscriberAsync(string email, int page = 1, int itemsPerPage = 10);
        Task<int> GetItemsBySubscriberTotalPagesAsync(string email, int itemsPerPage = 10);
        Task<Item> RemoveSubscriberFromItemAsync(Item item, string email);
        Task<Item> SearchItemByNameAndUrlAsync(string name, string url);
    }
}