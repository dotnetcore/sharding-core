using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ShardingCore.Core;
using ShardingCore.Core.VirtualRoutes;
using ShardingCore.Core.VirtualRoutes.TableRoutes;

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
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractSimpleShardingModKeyIntVirtualRoute<T>:AbstractShardingOperatorVirtualRoute<T,int> where T:class,IShardingEntity
    {
        protected readonly int Mod;
        protected readonly int TailLength;
        protected readonly char PaddingChar;
        protected AbstractSimpleShardingModKeyIntVirtualRoute(int tailLength,int mod,char paddingChar='0')
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
        protected override int ConvertToShardingKey(object shardingKey)
        {
            return Convert.ToInt32(shardingKey);
        }

        public override string ShardingKeyToTail(object shardingKey)
        {
            var shardingKeyInt = ConvertToShardingKey(shardingKey);
            return Math.Abs(shardingKeyInt % Mod).ToString().PadLeft(TailLength,PaddingChar);
        }

        public override List<string> GetAllTails()
        {
            return Enumerable.Range(0, Mod).Select(o => o.ToString().PadLeft(TailLength, PaddingChar)).ToList();
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(int shardingKey, ShardingOperatorEnum shardingOperator)
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