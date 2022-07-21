using System.Diagnostics;
using ShardingCore.Core.Collections;
using Xunit;

namespace ShardingCore.CommonTest
{
    public class CommonTest
    {
        [Fact]
        public void TestList()
        {
            var list = new List<string>(){"1","2","3"};
            var safeReadAppendList = new SafeReadAppendList<string>(list);
            Assert.True(safeReadAppendList.CopyList.Count==3);
            for (int i = 0; i < list.Count; i++)
            {
               Assert.Equal(list[i],safeReadAppendList.CopyList[i]);
            }
            list.Add("4");
            safeReadAppendList.Append("4");
            Assert.True(safeReadAppendList.CopyList.Count==4);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.Equal(list[i],safeReadAppendList.CopyList[i]);
            }

            Stopwatch sp = Stopwatch.StartNew();
            for (int i = 0; i < 1000000000; i++)
            {
                var copyListCount = safeReadAppendList.CopyList.Count;
            }
            sp.Stop();
            Console.WriteLine(sp.ElapsedMilliseconds);
        }
    }
    
}
