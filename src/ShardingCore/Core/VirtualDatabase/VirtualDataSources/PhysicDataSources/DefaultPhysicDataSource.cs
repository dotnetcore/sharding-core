using System;
using System.Collections.Generic;
using System.Text;

namespace ShardingCore.Core.VirtualDatabase.VirtualDataSources.PhysicDataSources
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/16 10:33:56
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class DefaultPhysicDataSource:IPhysicDataSource
    {
        public DefaultPhysicDataSource(string dsName, string connectionString, bool isDefault)
        {
            DataSourceName = dsName;
            ConnectionString = connectionString;
            IsDefault = isDefault;
        }



        public string DataSourceName { get; }
        public string ConnectionString { get; }
        public bool IsDefault { get; }


        protected bool Equals(DefaultPhysicDataSource other)
        {
            return DataSourceName == other.DataSourceName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DefaultPhysicDataSource)obj);
        }

        public override int GetHashCode()
        {
            return (DataSourceName != null ? DataSourceName.GetHashCode() : 0);
        }

    }
}
