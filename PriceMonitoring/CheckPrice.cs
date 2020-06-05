using System;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Dapper;
using System.Configuration;
using PriceMonitorData;
using System.Collections.Generic;

namespace PriceMonitoring
{
    public static class CheckPrice
    {
        [FunctionName("CheckPrice")]
        public static async Task Run([TimerTrigger("*/10 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            CosmosDB db;
            List<Item> items = new List<Item>();
            try
            {
                db = new CosmosDB();
                items = await db.GetAllItems();
            }
            catch(Exception ex)
            {
                log.LogInformation($"Can't access db: {ex.Message}");

                return;
            }
            
            foreach(Item item in items)
            {
                IDocument document = null;
                try
                {
                    document = await getPageAsync(new Url(item.Url));
                }
                catch
                {
                    log.LogInformation($"Page not loaded: {item.Url}");
                }

                var links = document.GetElementsByTagName("a").Where(x => x.InnerHtml.Contains(item.Name, StringComparison.OrdinalIgnoreCase));

                if (links.Count() == 0)
                {
                    log.LogInformation($"Item {item.Name} not found.");
                    return;
                }

                var priceStr = links.ElementAt(0).GetElementsByTagName("i").ElementAt(0).InnerHtml.Substring(2).Replace(" ", "");

                decimal currentPrice= 0;

                if (!decimal.TryParse(priceStr, out currentPrice))
                {
                    log.LogInformation($"Price can't be parsed: {item.Url}");
                    return;
                }

                log.LogInformation($"Item {item.Name} price: {currentPrice} rub.");

                Price priceInDB;
                try
                {
                    priceInDB = await db.GetItemPrice(item.Name);
                }
                catch
                {
                    await db.CreateItemPrice(item.Id, currentPrice);

                    return;
                }

                if(currentPrice < priceInDB.ItemPrice)
                {
                    //Notify users of price reductions
                }

                try
                {
                    await db.UpdateItemPrice(item.Id, currentPrice);
                }
                catch (Exception ex)
                {
                    log.LogInformation($"Can't save item {item.Name} price {currentPrice} to db: {ex.Message}.");
                }
            }
        }

        private static async Task<IDocument> getPageAsync(Url url)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader().WithDefaultCookies();

            var document = await BrowsingContext.New(config).OpenAsync(url);

            if (document.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"Bad document status: {document.StatusCode}");
            }

            return document;
        }

        public static IElement SearchForKeyWords(this IHtmlCollection<IElement> collection, string keyWord)
        {
            foreach (var element in collection)
            {
                if (element.InnerHtml.Contains(keyWord))
                {
                    return element;
                }
            }

            throw new Exception("There is no element containing neaded key words.");
        }
    }
}
