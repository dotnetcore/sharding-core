using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace ShardingCore.Sharding.PaginationConfigurations
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: 2021/9/2 7:45:55
    * @Ver: 1.0
    * @Email: 326308290@qq.com
    */
    public class PaginationConfig
    {
        public PaginationConfig(LambdaExpression orderPropertyExpression, PaginationMatchEnum paginationMatchEnum= PaginationMatchEnum.Owner, IComparer<string> tailComparer=null)
        {
            OrderPropertyExpression = orderPropertyExpression;
            OrderPropertyInfo = orderPropertyExpression.GetPropertyAccess();
            PropertyName = OrderPropertyInfo.Name;
            PaginationMatchEnum = paginationMatchEnum;
            TailComparer = tailComparer ?? Comparer<string>.Default;
        }

        public LambdaExpression OrderPropertyExpression { get; set; }
        public IComparer<string> TailComparer { get; set; }
        public PaginationMatchEnum PaginationMatchEnum { get; set; }
        public PropertyInfo OrderPropertyInfo { get; set; }

        /// <summary>
        /// 如果查询没发现排序就将当前配置追加上去
        /// </summary>
        public bool AppendIfOrderNone => AppendOrder >= 0;
        /// <summary>
        /// 大于等于0表示需要
        /// </summary>
        public int AppendOrder { get; set; } = -1;
        public string PropertyName { get;}


        protected bool Equals(PaginationConfig other)
        {
            return PropertyName == other.PropertyName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PaginationConfig) obj);
        }

        public override int GetHashCode()
        {
            return (PropertyName != null ? PropertyName.GetHashCode() : 0);
        }
    }
}
