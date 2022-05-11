using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Exceptions;
using ShardingCore.Infrastructures;
using ShardingCore.Sharding.ReadWriteConfigurations.Abstractions;

namespace ShardingCore.Sharding.ReadWriteConfigurations.Connectors.Abstractions
{
    public abstract class AbstractionReadWriteConnector:IReadWriteConnector
    {
        protected List<ReadNode> ReadNodes { get;}
        protected int Length { get; private set; }

        private object slock = new object();
        //private readonly string _tempConnectionString;
        //private readonly OneByOneChecker _oneByOneChecker = new OneByOneChecker();

        public AbstractionReadWriteConnector(string dataSourceName,ReadNode[] readNodes)
        {
            DataSourceName = dataSourceName;
            ReadNodes = readNodes.ToList();
            Length = ReadNodes.Count;
            //_tempConnectionString = ConnectionStrings[0];
        }
        public  string DataSourceName { get; }


        public string GetConnectionString(string readNodeName)
        {
            return DoGetConnectionString(readNodeName);
        }

        public abstract string DoGetConnectionString(string readNodeName);

        /// <summary>
        /// 动态添加数据源
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="readNodeName"></param>
        /// <returns></returns>
        public bool AddConnectionString(string connectionString, string readNodeName)
        {
            var acquired = Monitor.TryEnter(slock, TimeSpan.FromSeconds(3));
            if (!acquired)
                throw new ShardingCoreInvalidOperationException($"{nameof(AddConnectionString)} is busy");
            try
            {
                ReadNodes.Add(new ReadNode(readNodeName?? Guid.NewGuid().ToString("n"), connectionString));
                Length = ReadNodes.Count;
                return true;
            }
            finally
            {
                Monitor.Exit(slock);
            }
        }
    }
}
