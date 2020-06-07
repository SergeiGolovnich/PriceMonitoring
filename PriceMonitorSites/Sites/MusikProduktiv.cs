using AngleSharp;
using AngleSharp.Dom;
using PriceMonitorData;
using SimpleCache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorSites.Sites
{
    public class MusikProduktiv : ISite
    {
        public string HostName => "www.musik-produktiv.com";

        public async Task<decimal> ParsePrice(Item item, ICache<string, IDocument> cache = null)
        {
            var urlObj = new Url(item.Url);

            CheckHostName(urlObj);

            IDocument document;
            document = await GetCachedDocument(urlObj, cache);

            var links = document.GetElementsByTagName("a").Where(x => x.InnerHtml.Contains(item.Name, StringComparison.OrdinalIgnoreCase));

            if (links.Count() == 0)
            {
                throw new Exception($"Can't parse url: {item.Url}.");
            }

            var priceStr = links.ElementAt(0).GetElementsByTagName("i").ElementAt(0).InnerHtml.Substring(2).Replace(" ", "");

            decimal currentPrice = 0;

            if (!decimal.TryParse(priceStr, out currentPrice))
            {
                throw new Exception($"Price can't be parsed: {item.Url}");
            }

            return currentPrice;
        }

        private static async Task<IDocument> GetCachedDocument(Url urlObj, ICache<string, IDocument> cache)
        {
            IDocument document;

            if (cache == null)
            {
                document = await getPageAsync(urlObj);
            }
            else
            { 
                if (cache.Contains(urlObj.Href))
                {
                    document = cache.Get(urlObj.Href);
                }
                else
                {
                    document = await getPageAsync(urlObj);

                    cache.Add(urlObj.Href, document);
                }
            }

            return document;
        }

        private void CheckHostName(Url url)
        {
            if(url.HostName != HostName)
            {
                throw new Exception("Different hostname.");
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

        public static IElement SearchForKeyWords(IHtmlCollection<IElement> collection, string keyWord)
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
