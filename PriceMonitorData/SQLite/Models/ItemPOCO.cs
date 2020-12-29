using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriceMonitorData.Models;

namespace PriceMonitorData.SQLite.Models
{
    class ItemPOCO
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [Url]
        public string Url { get; set; }
        [Required]
        public string SubscribersEmailsString { get; set; }
        [NotMapped]
        public string[] SubscribersEmails {
            get { return JsonConvert.DeserializeObject<string[]>(SubscribersEmailsString); }
            set { SubscribersEmailsString = JsonConvert.SerializeObject(value); }
        }
    }
}
