using System;
using System.Collections.Generic;
using System.Text;

namespace PriceMonitorData
{
    public static class EnvHelper
    {
        public static string GetEnvironmentVariable(string name)
        {
            return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
