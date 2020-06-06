using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SimpleLRUCache
{/// <summary>
/// Last Used Cache
/// </summary>
    public class LUCache<TKey, TValue>
    {
        private int capacity;
        private Dictionary<TKey, TValue> dict;
        private LinkedList<TKey> list;
        public LUCache() : this(10)
        {

        }
        public LUCache(int capacity)
        {
            this.capacity = capacity;

            dict = new Dictionary<TKey, TValue>(capacity);

            list = new LinkedList<TKey>();
        }

        public void Add(TKey key, TValue value)
        {
            if (Contains(key))
            {
                return;
            }

            if(list.Count == capacity)
            {
                RemoveLastUsed();
            }

            dict.Add(key, value);

            list.AddFirst(key);
        }

        private void RemoveLastUsed()
        {
            dict.Remove(list.Last.Value);

            list.RemoveLast();
        }

        public TValue Get(TKey key)
        {
            TValue value;

            if(dict.TryGetValue(key, out value))
            {
                MoveKeyForward(key);

                return value;
            }
            else
            {
                throw new Exception($"No such item: {key}");
            }
        }

        private void MoveKeyForward(TKey key)
        {
            if(list.First.Value.Equals(key))
            {
                return;
            }

            list.Remove(key);
            list.AddFirst(key);
        }

        public bool Contains(TKey key)
        {
            return dict.ContainsKey(key);
        }
    }
}
