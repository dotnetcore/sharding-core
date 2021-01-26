using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;
using ShardingCore.Core;

namespace ShardingCore.Helpers
{
/*
* @Author: xjm
* @Description:
* @Date: Monday, 21 December 2020 12:34:55
* @Email: 326308290@qq.com
*/
/// <summary>
/// https://www.michael-whelan.net/replacing-appdomain-in-dotnet-core/
/// </summary>
    public class AssemblyHelper
    {
        private static readonly string AssemblyRoot = typeof(IShardingEntity).GetTypeInfo().Assembly.GetName().Name;
        public static AssemblyHelper CurrentDomain { get; private set; }

        static AssemblyHelper()
        {
            CurrentDomain = new AssemblyHelper();
        }

        // public Assembly[] GetAssemblies()
        // {
        //     return DependencyContext.Default.RuntimeLibraries.Where(IsCandidateLibrary)
        //         .SelectMany(l => l.GetDefaultAssemblyNames(DependencyContext.Default))
        //         .Select(assembly => Assembly.Load(new AssemblyName(assembly.Name)))
        //         .ToArray();
        // }
        public Assembly[] GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        private static bool IsCandidateLibrary(RuntimeLibrary library)
        {
            return library.Dependencies.Any(dependency => string.Equals(AssemblyRoot, dependency.Name, StringComparison.Ordinal));
        }
    }
}