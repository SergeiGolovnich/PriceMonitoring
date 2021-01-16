using PriceMonitorData.Models;
using PriceMonitorData.SQLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.SQLite
{
    public class SQLiteItemRepository : ItemRepository
    {
        private SQLiteContext context;
        public SQLiteItemRepository()
        {
            context = new SQLiteContext();
        }
        public async Task<Item> AddSubscriberToItemAsync(Item item, string email)
        {
            ItemPOCO itemInDb = context.Items.First(i => i.Id == item.Id);

            if (itemInDb.SubscribersEmails.Contains(email))
            {
                return itemInDb.ToItem();
            }

            itemInDb.SubscribersEmails = itemInDb.SubscribersEmails.Append(email).ToArray();

            context.Items.Update(itemInDb);

            await context.SaveChangesAsync();

            return itemInDb.ToItem();
        }

        public async Task<Item> CreateItemAsync(string itemName, string url, string[] emails)
        {
            var itemToCreate = new ItemPOCO
            {
                Id = Guid.NewGuid().ToString(),
                Name = itemName,
                Url = url,
                SubscribersEmails = emails
            };

            context.Items.Add(itemToCreate);

            await context.SaveChangesAsync();

            return itemToCreate.ToItem();

        }

        public async Task<Item> DeleteItemAsync(Item item)
        {
            var itempoco = context.Items.Find(item.Id);

            context.Items.Remove(itempoco);

            List<Price> prices = await GetAllItemPricesAsync(item);

            context.Prices.RemoveRange(prices);

            await context.SaveChangesAsync();

            return item;
        }
        private Task<List<Price>> GetAllItemPricesAsync(Item item)
        {
            List<Price> prices = context.Prices.Where(p => p.ItemId == item.Id).ToList();

            return Task.FromResult(prices);
        }

        public Task<List<Item>> GetAllItemsAsync(int page = 1, int itemsPerPage = 10)
        {
            var iterator = context.Items
               .OrderBy(x => x.Name)
               .Skip(itemsPerPage * (page - 1))
               .Take(itemsPerPage)
               .Select(i => i.ToItem());

            return Task.FromResult(iterator.ToList());
        }

        public Task<int> GetAllItemsTotalPagesAsync(int itemsPerPage = 10)
        {
            var itemsCount = context.Items.Count();

            int pages = itemsCount / itemsPerPage;

            if (itemsCount % itemsPerPage != 0)
            {
                pages++;
            }

            return Task.FromResult(pages);
        }

        public Task<Item> GetItemAsync(Item item)
        {
            return Task.FromResult(context.Items.First(i => i.Id == item.Id).ToItem());
        }

        public Task<Item> GetItemByIdAsync(string id)
        {
            var output = context.Items.First(i => i.Id == id);

            if (output == null)
            {
                throw new Exception($"There is no item with id: {id}.");
            }

            return Task.FromResult(output.ToItem());
        }

        public Task<List<Item>> GetItemsBySubscriberAsync(string email, int page = 1, int itemsPerPage = 10)
        {
            var iterator = context.Items
                .Where(i => i.SubscribersEmailsString.Contains(email))
                .OrderBy(x => x.Name)
                .Skip(itemsPerPage * (page - 1))
                .Take(itemsPerPage)
                .Select(i => i.ToItem());

            List<Item> output = iterator.ToList();

            return Task.FromResult(output);
        }

        public Task<int> GetItemsBySubscriberTotalPagesAsync(string email, int itemsPerPage = 10)
        {
            int itemsCount = context.Items
                .Where(i => i.SubscribersEmailsString.Contains(email))
                .Count();

            int pages = itemsCount / itemsPerPage;

            if (itemsCount % itemsPerPage != 0)
            {
                pages++;
            }

            return Task.FromResult(pages);
        }

        public async Task<Item> RemoveSubscriberFromItemAsync(Item item, string email)
        {
            var itemInDb = context.Items.First(i => i.Id == item.Id);

            if (!itemInDb.SubscribersEmails.Contains(email))
            {
                return itemInDb.ToItem();
            }

            if (itemInDb.SubscribersEmails.Contains(email))
            {
                var list = itemInDb.SubscribersEmails.ToList();

                list.Remove(email);

                itemInDb.SubscribersEmails = list.ToArray();
            }

            if (itemInDb.SubscribersEmails.Length == 0)
            {
                return await DeleteItemAsync(itemInDb.ToItem());
            }

            context.Items.Update(itemInDb);

            await context.SaveChangesAsync();

            return itemInDb.ToItem();
        }

        public Task<Item> SearchItemByNameAndUrlAsync(string name, string url)
        {
            var item = context.Items.First(i => i.Name == name && i.Url == url).ToItem();

            if (item == null)
            {
                throw new Exception($"There is no item with name: {name}, url: {url}.");
            }

            return Task.FromResult(item);
        }
    }
}
