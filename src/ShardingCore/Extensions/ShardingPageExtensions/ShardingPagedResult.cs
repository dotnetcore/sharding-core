using System.Collections.Generic;

namespace ShardingCore.Extensions.ShardingPageExtensions
{
    /// <summary>
    /// 分页集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ShardingPagedResult<T>
    {

        #region Ctor
        /// <summary>
        /// 初始化一个新的<c>PagedResult{T}</c>类型的实例。
        /// </summary>
        /// <param name="total">总记录数。</param>
        /// <param name="data">当前页面的数据。</param>
        public ShardingPagedResult(List<T> data, int total)
        {
            this.Total = total;
            this.Data = data;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// 获取或设置总记录数。
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 分页数据
        /// </summary>
        public List<T> Data { get; set; }
        #endregion
    }
}
