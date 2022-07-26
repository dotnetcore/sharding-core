using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.Internal
{
    public class FixedElementCollection<TEntity>
    {
        private readonly int _capacity;
        private readonly List<TEntity> _list;
        private int _virtualElementCount;
        public FixedElementCollection(int capacity)
        {
            _capacity = capacity;
            _virtualElementCount = 0;
            _list = new List<TEntity>(capacity);
        }

        public int VirtualElementCount => _virtualElementCount;
        public int ReallyElementCount => _list.Count;

        public bool Add(TEntity element)
        {
            _virtualElementCount++;
            if (VirtualElementCount <= _capacity)
            {
                _list.Add(element);
                return true;
            }
            return false;
        }

        public TEntity FirstOrDefault()
        {
            return _list.FirstOrDefault();
        }

        public TEntity First()
        {
            return _list.First();
        }
    }
}