using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ShardingCore.Sharding.Abstractions
{
/*
* @Author: xjm
* @Description:
* @Date: Saturday, 14 August 2021 21:47:11
* @Email: 326308290@qq.com
*/
    public interface IShardingDbContext
    {

        DbContext GetDbContext(string tail);
        
    }
}