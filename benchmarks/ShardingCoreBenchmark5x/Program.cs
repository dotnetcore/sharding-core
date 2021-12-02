using System;
using BenchmarkDotNet.Running;

namespace ShardingCoreBenchmark5x
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var result = BenchmarkRunner.Run<EFCoreCrud>();
            Console.ReadLine();
        }
    }
}
