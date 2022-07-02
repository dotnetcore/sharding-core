using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace ShardingCore.Core.Collections
{
    
    public class SafeReadAppendList<T>
    {
        private ImmutableList<T> _list;
        public SafeReadAppendList(IEnumerable<T> list)
        {
            _list = ImmutableList.CreateRange(list);
        }
        public SafeReadAppendList():this(new List<T>(0))
        {
        }

        public IReadOnlyList<T> Data => _list;

        public void Append(T value)
        {
            ImmutableList<T> original;
            ImmutableList<T> afterChange;
            do
            {
                original = _list!;
                afterChange = _list!.Add(value);
            }
            while (Interlocked.CompareExchange(ref _list, afterChange, original) != original);
        }

    }
}
