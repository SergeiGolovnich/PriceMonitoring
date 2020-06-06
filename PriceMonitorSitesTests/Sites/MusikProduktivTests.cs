using Xunit;
using PriceMonitorSites.Sites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleLRUCache;
using AngleSharp.Dom;

namespace PriceMonitorSites.Sites.Tests
{
    public class MusikProduktivTests
    {
        [Fact()]
        public async Task ParsePriceTest()
        {
            MusikProduktiv musikProduktiv = new MusikProduktiv();

            decimal price = await musikProduktiv.ParsePrice(@"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/",
                "Adam Audio T7V");

            Assert.True(price > 0, "Price can't be below zero.");
        }
        [Fact()]
        public async Task ParsePriceWithCacheEmptyTest()
        {
            MusikProduktiv musikProduktiv = new MusikProduktiv();
            var cache = new LUCache<string, IDocument>();

            decimal price = await musikProduktiv.ParsePrice(@"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/",
                "Adam Audio T7V", cache);

            Assert.True(price > 0, "Price can't be below zero.");
        }
        [Fact()]
        public async Task ParsePriceWithCacheNotEmptyTest()
        {
            MusikProduktiv musikProduktiv = new MusikProduktiv();
            var cache = new LUCache<string, IDocument>();

            decimal price = await musikProduktiv.ParsePrice(@"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/",
                "Adam Audio T7V", cache);

            Assert.True(cache.Contains(@"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/"), "Must contain document");

            price = await musikProduktiv.ParsePrice(@"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/",
                "Adam Audio T10S", cache);

            Assert.True(price > 0, "Price can't be below zero.");
        }
    }
}