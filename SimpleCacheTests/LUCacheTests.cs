using Xunit;
using SimpleLRUCache;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleLRUCache.Tests
{
    public class LUCacheTests
    {
        [Fact()]
        public void LUCacheDefaultTest()
        {
            LUCache<string, object> cache = new LUCache<string, object>();

            Assert.False(cache.Contains("Some key"), "Cache must be empty");
        }

        [Fact()]
        public void CanGetObject()
        {
            LUCache<string, object> cache = new LUCache<string, object>();

            object objExpected = new object();
            cache.Add("1", objExpected);

            object objActual = cache.Get("1");

            Assert.True(objActual.Equals(objExpected), "Must be same object");
        }

        [Fact()]
        public void AddTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void GetTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void ContainsTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}