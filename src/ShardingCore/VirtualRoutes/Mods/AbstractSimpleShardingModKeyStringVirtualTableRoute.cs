using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;
using ShardingCore.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ShardingCore.VirtualRoutes.Mods
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Wednesday, 27 January 2021 08:14:30
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 分表字段为string的取模分表
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class AbstractSimpleShardingModKeyStringVirtualTableRoute<TEntity>: AbstractShardingOperatorVirtualTableRoute<TEntity,string> where TEntity:class
    {
        protected readonly int Mod;
        protected readonly int TailLength;    
        /// <summary>
        /// 当取模后不足tailLength左补什么参数
        /// </summary>
        protected virtual char PaddingChar=>'0';

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tailLength">猴子长度</param>
        /// <param name="mod">取模被除数</param>
        protected AbstractSimpleShardingModKeyStringVirtualTableRoute(int tailLength,int mod)
        {
            if(tailLength<1)
                throw new ArgumentException($"{nameof(tailLength)} less than 1 ");
            if (mod < 1)
                throw new ArgumentException($"{nameof(mod)} less than 1 ");
            TailLength = tailLength;
            Mod = mod;
        }
        /// <summary>
        /// 如何将shardingkey转成对应的tail
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <returns></returns>
        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyStr = shardingKey.ToString();
            return Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKeyStr) % Mod).ToString().PadLeft(TailLength,PaddingChar);
        }
        /// <summary>
        /// 获取对应类型在数据库中的所有后缀
        /// </summary>
        /// <returns></returns>
        public override List<string> GetTails()
        {
            return Enumerable.Range(0, Mod).Select(o => o.ToString().PadLeft(TailLength, PaddingChar)).ToList();
        }
        /// <summary>
        /// 路由表达式如何路由到正确的表
        /// </summary>
        /// <param name="shardingKey"></param>
        /// <param name="shardingOperator"></param>
        /// <returns></returns>
        public override Func<string, bool> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {
            var t = ShardingKeyToTail(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
#if DEBUG
                    Console.WriteLine($"shardingOperator is not equal scan all table tail");           
#endif
                    return tail => true;
                }
            }
        }
    }
}