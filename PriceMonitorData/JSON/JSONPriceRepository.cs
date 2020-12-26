using PriceMonitorData.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.JSON
{
    class JSONPriceRepository : PriceRepository
    {
        public Task<Price> CreateItemPriceAsync(Item item, decimal price)
        {
            throw new NotImplementedException();
        }

        public Task<List<Price>> GetAllItemPricesAsync(Item item)
        {
            throw new NotImplementedException();
        }

        public Task<List<Price>> GetItemPricesAsync(Item item, int page = 1, int itemsPerPage = 10)
        {
            throw new NotImplementedException();
        }

        public Task<Price> GetLastItemPriceAsync(Item item)
        {
            throw new NotImplementedException();
        }

        public Task<List<Price>> GetLastItemPricesAsync(Item item, int maxCount = 1)
        {
            throw new NotImplementedException();
        }
    }
}
