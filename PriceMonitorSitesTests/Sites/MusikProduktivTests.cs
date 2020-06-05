using Xunit;
using PriceMonitorSites.Sites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
    }
}