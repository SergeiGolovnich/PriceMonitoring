using PriceMonitorData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData.SQLite.Models
{
    static class ItemPOCOExtentions
    {
        public static Item ToItem(this ItemPOCO poco)
        {
            return new Item()
            {
                Id = poco.Id,
                Name = poco.Name,
                SubscribersEmails = poco.SubscribersEmails,
                Url = poco.Url
            };
        }

        public static ItemPOCO ToItemPOCO(this Item item)
        {
            return new ItemPOCO()
            {
                Id = item.Id,
                Name = item.Name,
                SubscribersEmails = item.SubscribersEmails,
                Url = item.Url
            };
        }
    }
}
