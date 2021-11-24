using System;
using System.Threading.Tasks;

namespace ShardingCore.Jobs.Abstaractions
{
    internal interface IJob
    {
        string JobName { get; }
        string[] GetCronExpressions();
        Task ExecuteAsync();
        bool AutoCreateTableByTime();
    }
}
