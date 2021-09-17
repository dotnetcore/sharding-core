using System;
using System.Collections.Generic;
using System.Text;
using ShardingCore.Core.VirtualDataSources;

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
        public DefaultPhysicDataSource(string dsName, string connectionString, IVirtualDataSource virtualDataSource)
        {
            DSName = dsName;
            ConnectionString = connectionString;
            VirtualDataSource = virtualDataSource;
            EntityType = virtualDataSource.EntityType;
        }


        public string DSName { get; }
        public string ConnectionString { get; }
        public Type EntityType { get; }
        public IVirtualDataSource VirtualDataSource { get; }


        protected bool Equals(DefaultPhysicDataSource other)
        {
            return DSName == other.DSName;
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
            return (DSName != null ? DSName.GetHashCode() : 0);
        }
    }
}
