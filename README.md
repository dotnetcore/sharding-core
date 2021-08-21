<h1 align="center"> ShardingCore </h1>


`ShardingCore`是什么简单来说我认为他是目前最完美的efcore下的分表组件,支持所有的efcore支持的数据库(原则上),针对dbContext进行plus升级实现了无感知开发。
在使用程度上极大的降低了代码侵入性保证开发时候的无感知使用(和efcore原生dbcontext一样)。
该框架在分表数据下使用的而架构是流式聚合可以保证内存的稳定,而非内存迭代。
但是针对efcore下的框架基本上都适用,你以为这就完了吗shardingcore我可以说完美支持abp并且无侵入性。
最后如果喜欢本项目的话或者本项目对您有帮助的话麻烦[点我star github 地址](https://github.com/xuejmnet/sharding-core) ,也欢迎各位.neter交流。
### 依赖

Release  | EF Core | .NET Standard | .NET (Core) 
--- | --- | --- | --- 
[5.2.x.x](https://www.nuget.org/packages/ShardingCore/5.12.0.03) | >= 5.0.x | 2.1 | 3.0+ 
[3.2.x.x](https://www.nuget.org/packages/ShardingCore/3.2.0.03) | 3.1.10 | 2.0 | 2.0+ 
[2.2.x.x](https://www.nuget.org/packages/ShardingCore/2.2.0.03) | 2.2.6 | 2.0 | 2.0+ 
### 数据库支持 
数据库  | 是否支持 | 支持情况
--- | --- | --- 
SqlServer | 是 | 80%近乎完美
MySql |支持 | 未测试
PostgreSql | 支持 | 未测试
SQLite | 支持 | 未测试
Oracle | 支持 | 未测试
其他 | 支持(只要efcore支持) | 未测试



- [开始](#开始)
    - [简介](#简介)
    - [概念](#概念)
    - [优点](#优点)
    - [缺点](#缺点)
    - [安装](#安装)
    - [配置](#配置)
    - [使用](#使用)
    - [默认路由](#默认路由)
    - [Api](#Api)
- [高级配置](#高级配置)
    - [手动路由](#手动路由)
    - [自动建表](#自动建表)
    - [事务](#事务)
    - [批量操作](#批量操作)
- [注意事项](#注意事项)
- [计划(Future)](#计划)
- [最后](#最后)

# 开始

以下所有例子都以Sql Server为例 MySql亦如此


## 简介

该库从最初```2020年12月初```到现在还处于初期阶段,可能或许会有bug也希望各位多多理解,也是为了给.net生态贡献一下,从最初的仅支持单库分表到现在的多数据库分库分表且支持多表join和流式聚合等操作
开发该库也给我自己带了了很多新的编程思路,目前该库支持的分库分表可以进行完全的自定义,基本上可以满足95%以上的
业务需求，唯一的限制就是分表规则必须满足 x+y+z,x表示固定的表名,y表示固定的表名和表后缀之间的联系(可以为空),z表示表后缀,可以按照你自己的任意业务逻辑进行切分,
如:user_0,user_1或者user202101,user202102...当然该库同样适用于多租户模式下的隔离,该库为了支持之后的分库已经重写了之前的union all查询模式,并且支持多种api,
支持多种查询包括```join,group by,max,count,min,avg,sum``` ...等一系列查询,之后可能会添加更多支持,目前该库的使用非常简单,基本上就是针对IQueryable的扩展，为了保证
该库的简介目前仅使用该库无法或者说难以实现自动建表,但是只需要配合定时任务该库即可完成24小时无人看管自动管理。该库提供了 [IShardingTableCreator](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/TableCreator/IShardingTableCreator.cs)
作为建表的依赖,如果需要可以参考 [按天自动建表](https://github.com/xuejmnet/sharding-core/tree/main/samples/Samples.AutoByDate.SqlServer)

目前所有的demo可以参考 项目源码里面的Sample.SqlServer

## 概念

本库的几个简单的核心概念:

- [Tail]
  尾巴、后缀物理表的后缀
- [TailPrefix]
  尾巴前缀虚拟表和物理表的后缀中间的字符
- [物理表]
  顾名思义就是数据库对应的实际表信息,表名(tablename+ tailprefix+ tail) [IPhysicTable](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/PhysicTables/IPhysicTable.cs)
- [虚拟表]
  虚拟表就是系统将所有的物理表在系统里面进行抽象的一个总表对应到程序就是一个entity[IVirtualTable](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualTables/IVirtualTable.cs)
- [虚拟路由]
  虚拟路由就是联系虚拟表和物理表的中间介质,虚拟表在整个程序中只有一份,那么程序如何知道要查询系统哪一张表呢,最简单的方式就是通过虚拟表对应的路由[IVirtualTableRoute](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualRoutes/IVirtualRoute.cs)
  ,由于基本上所有的路由都是和业务逻辑相关的所以虚拟路由由用户自己实现,该框架提供一个高级抽象

## 优点

- [支持自定义分表规则]
- [支持任意类型分表key]
- [对dbcontext扩展近乎完美]
- [支持分表下的连表] ```join,group by,max,count,min,avg,sum```
- [支持针对批处理的使用] [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions) ...支持efcore的扩展生态
- [提供多种默认分表规则路由] 按时间,按取模 可自定义
- [针对分页进行优化] 大页数跳转支持低内存流式处理

## 缺点
- [暂不支持分库]
- [消耗连接]出现分表与分表对象进行join如果条件没法索引到具体表会生成```笛卡尔积```导致连接数爆炸,后期会进行针对该情况的配置
- [该库比较年轻] 可能会有一系列bug或者单元测试不到位的情况,但是只要你在群里或者提了issues我会尽快解决

## 安装
```xml
<PackageReference Include="ShardingCore" Version="5.2.0.03" />
```

## 配置

配置entity 推荐 [fluent api](https://docs.microsoft.com/en-us/ef/core/modeling/) 可以实现自动建表功能
`IShardingTable`数据库对象必须继承该接口
`ShardingTableKey`分表字段需要使用该特性

```c#
    public class SysUserMod:IShardingTable
    {
        /// <summary>
        /// 用户Id用于分表
        /// </summary>
        [ShardingTableKey]
        public string Id { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 用户姓名
        /// </summary>
        public int Age { get; set; }
    }
    
```
创建virtual route
实现 `AbstractShardingOperatorVirtualTableRoute<T, TKey>`
抽象,或者实现系统默认的虚拟路由
框架默认有提供几个简单的路由 [默认路由](#默认路由)

```c#

    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualRoute<SysUserMod>
    {
        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }
    }
```
创建DbContext必须继承IShardingTableDbContext

```c#

    public class DefaultTableDbContext: DbContext,IShardingTableDbContext
    {
        public DefaultTableDbContext(DbContextOptions<DefaultTableDbContext> options) :base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
        }

        public string ModelChangeKey { get; set; }
    }
```

创建分表DbContext必须继承AbstractShardingDbContext<DefaultTableDbContext>其中DefaultTableDbContext是你刚才建立的就是如果你分表了你真正获取对象是通过哪个dbcontext


```c#

  
    public class DefaultShardingDbContext:AbstractShardingDbContext<DefaultTableDbContext>
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
            modelBuilder.ApplyConfiguration(new SysTestMap());
        }

        public override Type ShardingDbContextType => this.GetType();
    }
```
`Startup.cs` 下的 `ConfigureServices(IServiceCollection services)`

```c#

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //services.AddDbContext<DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx3;Integrated Security=True"));

//添加shardingdbcontext support life scope
            services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx2;Integrated Security=True;MultipleActiveResultSets=True;")
                ,op =>
                {
                    op.EnsureCreatedWithOutShardingTable = true;
                    op.CreateShardingTableOnStart = true;
                    op.UseShardingOptionsBuilder((connection, builder) => builder.UseSqlServer(connection).UseLoggerFactory(efLogger));
                    op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                });
                
                
            // //不支持MARS不支持追踪的
            // services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx2;Integrated Security=True;")
            //     ,op =>
            //     {
            //         op.EnsureCreatedWithOutShardingTable = true;
            //         op.CreateShardingTableOnStart = true;
            //        //不支持mars额外加一条字符串的
            //        op.UseShardingOptionsBuilder(
            //            (connection, builder) => builder.UseSqlServer(connection).UseLoggerFactory(efLogger),
            //            (connString, builder) => builder.UseSqlServer(connString).UseLoggerFactory(efLogger));
            //         op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //     });
        }
```

`Startup.cs` 下的 ` Configure(IApplicationBuilder app, IWebHostEnvironment env)` 你也可以自行封装[app.UseShardingCore()](https://github.com/xuejmnet/sharding-core/blob/main/samples/Sample.SqlServer/DIExtension.cs)

```c#

            var shardingBootstrapper = app.ApplicationServices.GetRequiredService<IShardingBootstrapper>();
            shardingBootstrapper.Start();
```

## 使用
```c#
    
        private readonly DefaultShardingDbContext _defaultShardingDbContext;

        public ctor(DefaultShardingDbContext defaultShardingDbContext)
        {
            _defaultShardingDbContext = defaultShardingDbContext;
        }

        public async Task ToList_All()
        {
            //查询list集合
            var all=await _defaultShardingDbContext.Set<SysUserMod>().ToListAsync();
            //链接查询
            var list = await (from u in _defaultShardingDbContext.Set<SysUserMod>()
                join salary in _defaultShardingDbContext.Set<SysUserSalary>()
                    on u.Id equals salary.UserId
                select new
                {
                    Salary = salary.Salary,
                    DateOfMonth = salary.DateOfMonth,
                    Name = u.Name
                }).ToListAsync();
            //聚合查询
            var ids = new[] {"200", "300"};
            var dateOfMonths = new[] {202111, 202110};
            var group = await (from u in _defaultShardingDbContext.Set<SysUserSalary>()
                    .Where(o => ids.Contains(o.UserId) && dateOfMonths.Contains(o.DateOfMonth))
                group u by new
                {
                    UId = u.UserId
                }
                into g
                select new
                {
                    GroupUserId = g.Key.UId,
                    Count = g.Count(),
                    TotalSalary = g.Sum(o => o.Salary),
                    AvgSalary = g.Average(o => o.Salary),
                    MinSalary = g.Min(o => o.Salary),
                    MaxSalary = g.Max(o => o.Salary)
                }).ToListAsync();
        }
```
更多操作可以参考单元测试

## Api

方法  | Method | [Unit Test](https://github.com/xuejmnet/sharding-core/blob/main/test/ShardingCore.Test50/ShardingTest.cs) 
--- |--- |--- 
获取集合 |ToListAsync |yes 
第一条 |FirstOrDefaultAsync |yes 
最大 |MaxAsync |yes 
最小 |MinAsync |yes 
是否存在 |AnyAsync |yes 
数目 |CountAsync |yes 
数目 |LongCountAsync |yes 
求和 |SumAsync |yes 
平均 |AverageAsync |yes 
包含 |ContainsAsync |yes 
分组 |GroupByAsync |yes 

## 默认路由

抽象abstract  | 路由规则 | tail | 索引
--- |--- |--- |--- 
AbstractSimpleShardingModKeyIntVirtualTableRoute |取模 |0,1,2... | `=`
AbstractSimpleShardingModKeyStringVirtualTableRoute |取模 |0,1,2... | `=`
AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute |按时间 |yyyyMMdd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingDayKeyLongVirtualTableRoute |按时间戳 |yyyyMMdd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingWeekKeyDateTimeVirtualTableRoute |按时间 |yyyyMMdd_dd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingWeekKeyLongVirtualTableRoute |按时间戳 |yyyyMMdd_dd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute |按时间 |yyyyMM | `>,>=,<,<=,=,contains`
AbstractSimpleShardingMonthKeyLongVirtualTableRoute |按时间戳 |yyyyMM | `>,>=,<,<=,=,contains`
AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute |按时间 |yyyy | `>,>=,<,<=,=,contains`
AbstractSimpleShardingYearKeyLongVirtualTableRoute |按时间戳 |yyyy | `>,>=,<,<=,=,contains`

注:`contains`表示为`o=>ids.contains(o.shardingkey)`

#高级

## 批量操作

批量操作将对应的dbcontext和数据进行分离由用户自己选择第三方框架比如zzz进行批量操作或者batchextension
```c#
后期支持
```
## 手动路由
```c#
 后续支出
```

## 自动建表
[参考](https://github.com/xuejmnet/sharding-core/tree/main/samples/Samples.AutoByDate.SqlServer)

## 事务
默认savechanges支持事务如果需要where.update需要手动开启事务
```c#
1.
 await  _defaultShardingDbContext.SaveChangesAsync();
 2.
            var tran=_defaultShardingDbContext.BeginTransaction();
 await  _defaultShardingDbContext.SaveChangesAsync();
 tran.commit()
           
```

# 注意事项
该库的追踪是基于adonet的MARS(MultipleActiveResultSets=True;)所以基本不支持该特性的无法支持完美追踪
,目前框架采用AppDomain.CurrentDomain.GetAssemblies();
可能会导致程序集未被加载所以尽可能在api层加载所需要的dll
使用时需要注意
- 实体对象是否继承`IShardingTable`
- 实体对象是否有`ShardingKey`
- 实体对象是否已经实现了一个虚拟路由
- startup是否已经添加虚拟路由
- startup是否已经添加bootstrapper.start()

```c#
            // //不支持MARS不支持追踪的
            // services.AddShardingDbContext<DefaultShardingDbContext, DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBxx2;Integrated Security=True;")
            //     ,op =>
            //     {
            //         op.EnsureCreatedWithOutShardingTable = true;
            //         op.CreateShardingTableOnStart = true;
            //        //不支持mars额外加一条字符串的
            //        op.UseShardingOptionsBuilder(
            //            (connection, builder) => builder.UseSqlServer(connection).UseLoggerFactory(efLogger),
            //            (connString, builder) => builder.UseSqlServer(connString).UseLoggerFactory(efLogger));
            //         op.AddShardingTableRoute<SysUserModVirtualTableRoute>();
            //     });
```


# 计划
- [提供官网如果该项目比较成功的话]
- [开发更完善的文档]
- [支持分库]
- [支持更多数据库查询完善]

# 最后
该框架借鉴了大部分分表组件的思路,目前提供的接口都已经实现,并且支持跨表查询,基于分页查询该框架也使用了流式查询保证不会再skip大数据的时候内存会爆炸,目前这个库只是一个刚刚成型的库还有很多不完善的地方希望大家多多包涵,如果喜欢的话也希望大家给个star.
该文档是我晚上赶工赶出来的也想趁热打铁希望更多的人关注,也希望更多的人可以交流。

凭借各大开源生态圈提供的优秀代码和思路才有的这个框架,希望可以为.Net生态提供一份微薄之力,该框架本人会一直长期维护,有大神技术支持可以联系下方方式欢迎star :)

[博客](https://www.cnblogs.com/xuejiaming)

QQ群:771630778

个人QQ:326308290(欢迎技术支持提供您宝贵的意见)

个人邮箱:326308290@qq.com