using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleCache
{
    public interface ICache<TKey, TValue>
    {
        void Add(TKey key, TValue value);
        TValue Get(TKey key);
        bool Contains(TKey key);
        void Clear();
    }
}
