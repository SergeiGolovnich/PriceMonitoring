using Xunit;
using PriceMonitorSites;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorSites.Tests
{
    public class PriceParserTests
    {
        [Theory()]
        [InlineData(@"https://www.musik-produktiv.com/ru/studijnye-monitory/adam-audio/", "Adam Audio T7V")]
        public async Task ParseTest(string url, string searchPhrase)
        {
            var price = await PriceParser.Parse(url, searchPhrase);

            Assert.True(price > 0, "Price can't be below zero.");
        }
    }
}