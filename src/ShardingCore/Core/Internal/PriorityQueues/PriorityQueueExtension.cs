using System;

namespace ShardingCore.Core.Internal.PriorityQueues
{
    /*
    * @Author: xjm
    * @Description:
    * @Date: Monday, 25 January 2021 10:10:04
    * @Email: 326308290@qq.com
    */
    /// <summary>
    /// 优先级队列扩展方法
    /// </summary>
    internal static class PriorityQueueExtension
    {
        /// <summary>
        /// 返回默认第一个元素,不删除元素,集合为空返回null
        /// </summary>
        /// <param name="queue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Peek<T>(this PriorityQueue<T> queue)
        {
            if (queue.IsEmpty())
                return default(T);
            return queue.Top();
        }
        /// <summary>
        /// 返回并删除第一个元素集合为空返回null
        /// </summary>
        /// <param name="queue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Poll<T>(this PriorityQueue<T> queue)
        {
            if (queue.IsEmpty())
                return default(T);
            var first = queue.Top();
            queue.Pop();
            return first;
        }
        /// <summary>
        /// 向队列添加元素如果集合满了返回false
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="element"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        public static bool Offer<T>(this PriorityQueue<T> queue, T element)
        {
            if (queue.IsFull())
                return false;
            queue.Push(element);
            return true;
        }
    }
}