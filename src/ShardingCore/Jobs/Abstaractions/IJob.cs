using System;
using System.Threading.Tasks;

namespace ShardingCore.Jobs.Abstaractions
{
    public interface IJob
    {
        string JobName { get; }
        string[] GetCronExpressions();
        Task ExecuteAsync();
        bool AppendJob();
    }
}
