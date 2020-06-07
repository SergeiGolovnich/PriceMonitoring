using AngleSharp.Browser.Dom;
using AngleSharp.Dom;
using PriceMonitorData;
using SimpleCache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorSites.Sites
{
    interface ISite
    {
        string HostName { get; }
        Task<decimal> ParsePrice(Item item, ICache<string, IDocument> cache = null);
    }
}
