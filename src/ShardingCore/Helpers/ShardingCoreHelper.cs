namespace ShardingCore.Helpers
{
/*
* @Author: xjm
* @Description:
* @Date: Friday, 22 January 2021 13:32:08
* @Email: 326308290@qq.com
*/
    public class ShardingCoreHelper
    {
        private ShardingCoreHelper(){}
        public static int GetStringHashCode(string value)
        {
            int h = 0; // 默认值是0
            if (value.Length > 0) {
                for (int i = 0; i < value.Length; i++) {
                    h = 31 * h + value[i]; // val[0]*31^(n-1) + val[1]*31^(n-2) + ... + val[n-1]
                }
            }
            return h;
        }
    }
}