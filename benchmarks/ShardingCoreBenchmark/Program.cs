// See https://aka.ms/new-console-template for more information

using BenchmarkDotNet.Running;
using ShardingCore6x;

var result = BenchmarkRunner.Run<EFCoreCrud>();

Console.ReadLine();
