using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShardingCore.Extensions
{
    public static class StreamMergeEnumerableExtension
    {
        public static async Task<List<TEntity>> ToStreamListAsync<TEntity>(this IAsyncEnumerable<TEntity> source, int? take=null,CancellationToken cancellationToken=default)
        {
#if EFCORE2
            var list = await asyncEnumeratorStreamMergeEngine.ToList<TEntity>(cancellationToken);
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
    }
}
