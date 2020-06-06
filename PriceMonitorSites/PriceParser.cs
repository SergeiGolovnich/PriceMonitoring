using AngleSharp.Dom;
using PriceMonitorSites.Sites;
using SimpleCache;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace PriceMonitorSites
{
    public static class PriceParser
    {
        public static async Task<decimal> Parse(string url, string searchPhrase, ICache<string, IDocument> cache = null)
        {
            var asm = Assembly.GetExecutingAssembly();

            Type typeISite = asm.GetType("PriceMonitorSites.Sites.ISite");

            var types = asm.DefinedTypes.Where(x => x.IsClass && x.ImplementedInterfaces.Contains(typeISite)).Select(x => x.AsType());

            decimal price = 0;

            foreach (var siteParser in types)
            {
                try
                {
                    object parser = Activator.CreateInstance(siteParser);

                    price = await (parser as ISite).ParsePrice(url, searchPhrase, cache);

                    return price;
                }
                catch { }
            }

            throw new Exception("None of the parsers worked.");
        }
    }
}
