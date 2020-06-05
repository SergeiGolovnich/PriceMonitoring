using AngleSharp;
using AngleSharp.Dom;
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

        public async Task<decimal> ParsePrice(string url, string searchPhrase)
        {
            var urlObj = new Url(url);

            CheckHostName(urlObj);

            IDocument document = null;

            document = await getPageAsync(urlObj);

            var links = document.GetElementsByTagName("a").Where(x => x.InnerHtml.Contains(searchPhrase, StringComparison.OrdinalIgnoreCase));

            if (links.Count() == 0)
            {
                throw new Exception($"Can't parse url: {url}.");
            }

            var priceStr = links.ElementAt(0).GetElementsByTagName("i").ElementAt(0).InnerHtml.Substring(2).Replace(" ", "");

            decimal currentPrice = 0;

            if (!decimal.TryParse(priceStr, out currentPrice))
            {
                throw new Exception($"Price can't be parsed: {url}");
            }

            return currentPrice;
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
