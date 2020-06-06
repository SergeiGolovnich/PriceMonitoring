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
        public void CanGetObjectTest()
        {
            LUCache<string, object> cache = new LUCache<string, object>();

            object objExpected = new object();
            cache.Add("1", objExpected);

            object objActual = cache.Get("1");

            Assert.True(objActual.Equals(objExpected), "Must be same object");
        }

        [Fact()]
        public void WrongCapacityTest()
        {
            LUCache<string, object> cache;
            try
            {
                cache = new LUCache<string, object>(-1);
            }catch(Exception ex)
            {
                Assert.IsType<ArgumentException>(ex);
            }
        }

        [Fact()]
        public void DontContainsSqueezedOutTest()
        {
            LUCache<string, object> cache = new LUCache<string, object>(1);

            cache.Add("1", new object());
            cache.Add("2", new object());

            Assert.False(cache.Contains("1"), "Must not contain a discarded element");
            Assert.True(cache.Contains("2"), "Must contain last added element");
        }
        [Fact()]
        public void ClearTest()
        {
            LUCache<string, object> cache = new LUCache<string, object>(1);

            cache.Add("1", new object());

            Assert.True(cache.Contains("1"), "Must contain last added element");

            cache.Clear();

            Assert.False(cache.Contains("1"), "Cleared cache must not contains elements");
        }
    }
}