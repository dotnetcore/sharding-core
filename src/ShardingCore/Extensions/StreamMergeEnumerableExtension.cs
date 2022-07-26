using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShardingCore.Core.Internal;

namespace ShardingCore.Extensions
{
    public static class StreamMergeEnumerableExtension
    {
        public static async Task<List<TEntity>> ToStreamListAsync<TEntity>(this IAsyncEnumerable<TEntity> source, int? take=null,CancellationToken cancellationToken=default)
        {
#if EFCORE2
            var list = await source.ToList<TEntity>(cancellationToken);
#endif
#if !EFCORE2
            var list = new List<TEntity>(take??4);
            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                list.Add(element);
            }
#endif
            return list;
        }
        public static List<TEntity> ToStreamList<TEntity>(this IEnumerable<TEntity> source)
        {
            return source.ToList();
        }
        public static FixedElementCollection<TEntity> ToFixedElementStreamList<TEntity>(this IEnumerable<TEntity> source,int capacity,int maxVirtualElementCount)
        {
            var fixedElementCollection = new FixedElementCollection<TEntity>(capacity);
            using var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                fixedElementCollection.Add(enumerator.Current);
                if (fixedElementCollection.VirtualElementCount >= maxVirtualElementCount)
                {
                    break;
                }
            }
            return fixedElementCollection;
        }
        
        
        public static async Task<FixedElementCollection<TEntity>> ToFixedElementStreamListAsync<TEntity>(this IAsyncEnumerable<TEntity> source,int capacity,int maxVirtualElementCount,CancellationToken cancellationToken=default)
        {
            var fixedElementCollection = new FixedElementCollection<TEntity>(capacity);
#if EFCORE2
            var asyncEnumerator = source.GetEnumerator();
            while (await asyncEnumerator.MoveNext(cancellationToken))
            {
                fixedElementCollection.Add(asyncEnumerator.Current);
                if (fixedElementCollection.VirtualElementCount >= maxVirtualElementCount)
                {
                    break;
                }
            }
#endif
#if !EFCORE2
            await foreach (var element in source.WithCancellation(cancellationToken))
            {
                fixedElementCollection.Add(element);
                if (fixedElementCollection.VirtualElementCount >= maxVirtualElementCount)
                {
                    break;
                }
            }
#endif
            return fixedElementCollection;
        }
    }
}
