using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ShardingCore.Extensions.InternalExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// Author: xjm
    /// Created: 2022/4/30 21:44:22
    /// Email: 326308290@qq.com
    internal  static class InternalLogExtension
    {
        public static void LogLazyDebug(this ILogger logger, Func<string> msgCreator)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug(msgCreator());
            }
        }
    }
}
