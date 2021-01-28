using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceMonitorData
{
    public interface PriceCheckerService
    {
        Task CheckPricesAsync();
        DateTime LastCheckTime { get; }
        bool IsChecking { get; }
        bool IsActive { get; set; }
        TimeSpan Interval { get; set; }
        TimeSpan TimeSpentChecking { get; }
        IList<string> Errors { get; }
    }
}
