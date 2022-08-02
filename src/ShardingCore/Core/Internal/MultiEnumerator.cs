using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ShardingCore.Core.Internal
{

    public class MultiEnumerator<TEntity>:IEnumerator<TEntity>
    {
        private readonly List<IEnumerator<TEntity>> _enumerators;
        private int index;
        private readonly int _enumeratorsCount;

        public MultiEnumerator(IEnumerable<IEnumerator<TEntity>> enumerators)
        {
            _enumerators = enumerators.ToList();
            _enumeratorsCount = _enumerators.Count;
            index = 0;
        }
        public bool MoveNext()
        {
            if (_enumeratorsCount == 0)
            {
                return false;
            }

            if (index >= _enumeratorsCount)
            {
                return false;
            }

            while (index < _enumeratorsCount)
            {
                var moveNext = _enumerators[index].MoveNext();
                if (moveNext)
                {
                    return true;
                }
                index++;
            }
            return false;
        }

        public void Reset()
        {
            throw new System.NotImplementedException();
        }

        public TEntity Current => _enumerators[index].Current;

        object IEnumerator.Current => Current;

        public void Dispose()
        {
            _enumerators.Clear();
        }
    }
}