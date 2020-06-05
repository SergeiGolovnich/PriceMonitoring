using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PriceMonitorData
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
