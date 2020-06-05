using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PriceMonitorData
{
    public class Item
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }

        public string Url { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
