using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.EntityMetadatas;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes.Abstractions;

namespace ShardingCore.VirtualRoutes.Mods
{
/*
* @Author: xjm
* @Description:
* @Date: Wednesday, 27 January 2021 08:21:25
* @Email: 326308290@qq.com
*/
    /// <summary>
    /// 分表字段为int的取模分表
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public abstract class AbstractSimpleShardingModKeyIntVirtualTableRoute<TEntity>: AbstractShardingOperatorVirtualTableRoute<TEntity,int> where TEntity:class
    {
        protected readonly int Mod;
        protected readonly int TailLength;
        protected readonly char PaddingChar;
        protected AbstractSimpleShardingModKeyIntVirtualTableRoute(int tailLength,int mod,char paddingChar= '0') 
        {
            if(tailLength<1)
                throw new ArgumentException($"{nameof(tailLength)} less than 1 ");
            if (mod < 1)
                throw new ArgumentException($"{nameof(mod)} less than 1 ");
            if (string.IsNullOrWhiteSpace(paddingChar.ToString()))
                throw new ArgumentException($"{nameof(paddingChar)} cant empty ");
            TailLength = tailLength;
            Mod = mod;
            PaddingChar = paddingChar;
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyInt = Convert.ToInt32(shardingKey);
            return Math.Abs(shardingKeyInt % Mod).ToString().PadLeft(TailLength,PaddingChar);
        }

        public override List<string> GetAllTails()
        {
            return Enumerable.Range(0, Mod).Select(o => o.ToString().PadLeft(TailLength, PaddingChar)).ToList();
        }

        public override Func<string, bool> GetRouteToFilter(int shardingKey, ShardingOperatorEnum shardingOperator)
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