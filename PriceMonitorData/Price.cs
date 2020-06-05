using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace PriceMonitorData
{
    public class Price
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string ItemName { get; set; }

        public decimal ItemPrice { get; set; }

        public DateTime Date { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
