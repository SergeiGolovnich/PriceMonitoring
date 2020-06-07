using Xunit;
using PriceMonitorSites.Sites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SimpleLRUCache;
using AngleSharp.Dom;
using PriceMonitorData;

namespace PriceMonitorSites.Sites.Tests
{
    public class MusikProduktivTests
    {
        private Item testItem = new Item
        {
            Name = "Adam Audio T7V",
            Url = @"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/"
        };
        private Item testItem2 = new Item
        {
            Name = "Adam Audio T10S",
            Url = @"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/"
        };

        [Fact()]
        public async Task ParsePriceTest()
        {
            MusikProduktiv musikProduktiv = new MusikProduktiv();

            decimal price = await musikProduktiv.ParsePrice(testItem);

            Assert.True(price > 0, "Price can't be below zero.");
        }
        [Fact()]
        public async Task ParsePriceWithCacheEmptyTest()
        {
            MusikProduktiv musikProduktiv = new MusikProduktiv();
            var cache = new LUCache<string, IDocument>();

            decimal price = await musikProduktiv.ParsePrice(testItem, cache);

            Assert.True(price > 0, "Price can't be below zero.");
        }
        [Fact()]
        public async Task ParsePriceWithCacheNotEmptyTest()
        {
            MusikProduktiv musikProduktiv = new MusikProduktiv();
            var cache = new LUCache<string, IDocument>();

            decimal price = await musikProduktiv.ParsePrice(testItem, cache);

            Assert.True(cache.Contains(testItem.Url), "Must contain document");

            price = await musikProduktiv.ParsePrice(testItem2, cache);

            Assert.True(price > 0, "Price can't be below zero.");
        }
    }
}