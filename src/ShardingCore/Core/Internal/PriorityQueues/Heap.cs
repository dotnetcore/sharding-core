using System.Collections.Generic;

namespace ShardingCore.Core.Internal.PriorityQueues {
    /// <summary>
    /// 大顶堆
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class Heap<T> {
        public static void HeapSort(T[] objects,IComparer<T> comparer) {
            HeapSort(objects, false,comparer);
        }
        public static void HeapSort(T[] objects, bool descending,IComparer<T> comparer) {
            for (int i = objects.Length / 2 - 1; i >= 0; --i)
                heapAdjustFromTop(objects, i, objects.Length, descending,comparer);
            for (int i = objects.Length - 1; i > 0; --i) {
                swap(objects, i, 0);
                heapAdjustFromTop(objects, 0, i, descending,comparer);
            }
        }

        public static void heapAdjustFromBottom(T[] objects, int n,IComparer<T> comparer) {
            heapAdjustFromBottom(objects, n, false,comparer);
        }

        public static void heapAdjustFromBottom(T[] objects, int n, bool descending,IComparer<T> comparer) {
            while (n > 0 && descending ^ comparer.Compare(objects[(n - 1) >> 1],objects[n]) < 0) {
                swap(objects, n, (n - 1) >> 1);
                n = (n - 1) >> 1;
            }
        }

        public static void heapAdjustFromTop(T[] objects, int n, int len,IComparer<T> comparer) {
            heapAdjustFromTop(objects, n, len, false,comparer);
        }

        public static void heapAdjustFromTop(T[] objects, int n, int len, bool descending,IComparer<T> comparer) {
            while ((n << 1) + 1 < len) {
                int m = (n << 1) + 1;
                if (m + 1 < len && descending ^ comparer.Compare(objects[m],objects[m + 1]) < 0)
                    ++m;
                if (descending ^ comparer.Compare(objects[n],objects[m])> 0) return;
                swap(objects, n, m);
                n = m;
            }
        }

        private static void swap(T[] objects, int a, int b) {
            T tmp = objects[a];
            objects[a] = objects[b];
            objects[b] = tmp;
        }
    }
}
