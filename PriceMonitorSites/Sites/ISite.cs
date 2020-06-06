using AngleSharp.Browser.Dom;
using AngleSharp.Dom;
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
        Task<decimal> ParsePrice(string url, string searchPhrase, ICache<string, IDocument> cache = null);
    }
}
