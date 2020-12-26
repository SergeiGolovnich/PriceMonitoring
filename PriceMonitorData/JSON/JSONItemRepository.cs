using PriceMonitorData.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.JSON
{
    public class JSONItemRepository : ItemRepository
    {
        public Task<Item> AddSubscriberToItemAsync(Item item, string email)
        {
            throw new NotImplementedException();
        }

        public Task<Item> CreateItemAsync(string itemName, string url, string[] emails)
        {
            throw new NotImplementedException();
        }

        public Task<Item> DeleteItemAsync(Item item)
        {
            throw new NotImplementedException();
        }

        public Task<List<Item>> GetAllItemsAsync(int page = 1, int itemsPerPage = 10)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetAllItemsTotalPagesAsync(int itemsPerPage = 10)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItemAsync(Item item)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItemByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Item>> GetItemsBySubscriberAsync(string email, int page = 1, int itemsPerPage = 10)
        {
            throw new NotImplementedException();
        }

        public Task<int> GetItemsBySubscriberTotalPagesAsync(string email, int itemsPerPage = 10)
        {
            throw new NotImplementedException();
        }

        public Task<Item> RemoveSubscriberFromItemAsync(Item item, string email)
        {
            throw new NotImplementedException();
        }

        public Task<Item> SearchItemByNameAndUrlAsync(string name, string url)
        {
            throw new NotImplementedException();
        }
    }
}
