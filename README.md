<h1 align="center"> ShardingCore </h1>


`ShardingCore` 易用、简单、高性能、普适性，是一款扩展针对efcore生态下的分表分库的扩展解决方案,支持efcore2+的所有版本,支持efcore2+的所有数据库、支持自定义路由、动态路由、高性能分页、读写分离的一款组件，如果你喜欢这组件或者这个组件对你有帮助请点击下发star让更多的.neter可以看到使用

---

<div align="center">
<p><a href="https://github.com/xuejmnet/sharding-core">Github Source Code</a>  助力dotnet 生态 <a href="https://gitee.com/dotnetchina/sharding-core">Gitee Source Code</a> </p>
</div>

---

## 📚 Documentation

[中文文档](https://xuejmnet.github.io/sharding-core-doc/) | [English Document](https://xuejmnet.github.io/sharding-core-doc/en/)
### 依赖

Release  | EF Core | .NET Standard | .NET (Core) 
--- | --- | --- | --- 
[5.x.x.x](https://www.nuget.org/packages/ShardingCore) | >= 5.0.10 | 2.1 | 3.0+ 
[3.x.x.x](https://www.nuget.org/packages/ShardingCore) | 3.1.18 | 2.0 | 2.0+ 
[2.x.x.x](https://www.nuget.org/packages/ShardingCore) | 2.2.6 | 2.0 | 2.0+ 
### 数据库支持 
数据库  | 是否支持 | 支持情况
--- | --- | --- 
SqlServer | 支持 | 支持
MySql |支持 | 支持
PostgreSql | 支持 | 支持
SQLite | 支持 | 未测试
Oracle | 支持 | 未测试
其他 | 支持(只要efcore支持) | 未测试



- [使用介绍](#使用介绍)
    - [简介](#简介)
    - [概念](#概念)
    - [优点](#优点)
    - [缺点](#缺点)
    - [安装](#安装)
- [开始](#开始)
    - [分表](#分表)
    - [分库](#分库)
    - [默认路由](#默认路由)
    - [Api](#Api)
- [高级配置](#高级配置)
    - [code-first](#code-first)
    - [自动追踪](#自动追踪)
    - [手动路由](#手动路由)
    - [自动建表](#自动建表)
    - [事务](#事务)
    - [批量操作](#批量操作)
    - [读写分离](#读写分离)
    - [高性能分页](#高性能分页)
- [注意事项](#注意事项)
- [计划(Future)](#计划)
- [最后](#最后)

# 使用介绍

以下所有例子都以Sql Server为例 展示的代码均是分表为例,如果需要分库可以参考[Sample.SqlServerShardingDataSource](https://github.com/xuejmnet/sharding-core/tree/main/samples/Sample.SqlServerShardingDataSource) 其他数据库亦是如此


## 简介

简单介绍下这个库,这个库的所有版本都是由对应的efcore版本号为主的版本，第二个版本号如果是2的表示仅支持分库,如果是3+的表示支持分库分表，这个库目前分成两个主要版本一个是main分支一个是shardingTableOnly分支,该库支持分库完全自定义路由适用于95%的业务需求,分表支持x+y+z,x表示固定的表名,y表示固定的表名和表后缀之间的联系(可以为空),z表示表后缀,可以按照你自己的任意业务逻辑进行切分,
如:user_0,user_1或者user202101,user202102...当然该库同样适用于多租户模式下的隔离
支持多种查询包括```join,group by,max,count,min,avg,sum``` ...等一系列查询,之后可能会添加更多支持,目前该库的使用非常简单,基本上就是针对IQueryable的扩展，为了保证
该库的干净零依赖,如果需要实现自动建表需要自己配合定时任务,即可完成24小时无人看管自动管理。该库提供了 [IShardingTableCreator](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/TableCreator/IShardingTableCreator.cs)
作为建表的依赖,如果需要可以参考 [按天自动建表](https://github.com/xuejmnet/sharding-core/tree/main/samples/Samples.AutoByDate.SqlServer) 该demo是针对分库的动态添加

## 概念

本库的几个简单的核心概念:

### 分库概念
- [DataSourceName]
  数据源名称用来将对象路由到具体的数据源
- [IVirtualDataSource]
  虚拟数据源 [IVirtualDataSource](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualDatabase/VirtualDataSources/IVirtualDataSource.cs)
- [IVirtualDataSourceRoute]
  分库路由  [IVirtualDataSourceRoute](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualRoutes/DataSourceRoutes/IVirtualDataSourceRoute.cs)
### 分表概念
- [Tail]
  尾巴、后缀物理表的后缀
- [TailPrefix]
  尾巴前缀虚拟表和物理表的后缀中间的字符
- [物理表]
  顾名思义就是数据库对应的实际表信息,表名(tablename+ tailprefix+ tail) [IPhysicTable](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/PhysicTables/IPhysicTable.cs)
- [虚拟表]
  虚拟表就是系统将所有的物理表在系统里面进行抽象的一个总表对应到程序就是一个entity[IVirtualTable](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualDatabase/VirtualTables/IVirtualTable.cs)
- [虚拟路由]
  虚拟路由就是联系虚拟表和物理表的中间介质,虚拟表在整个程序中只有一份,那么程序如何知道要查询系统哪一张表呢,最简单的方式就是通过虚拟表对应的路由[IVirtualTableRoute](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualRoutes/TableRoutes/IVirtualTableRoute.cs)
  ,由于基本上所有的路由都是和业务逻辑相关的所以虚拟路由由用户自己实现,该框架提供一个高级抽象

## 优点

- [支持自定义分库]
- [支持读写分离]
- [支持高性能分页]
- [支持手动路由]
- [支持批量操作]
- [支持自定义分表规则]
- [支持任意类型分表key]
- [对dbcontext学习成本0]
- [支持分表下的连表] ```join,group by,max,count,min,avg,sum```
- [支持针对批处理的使用] [EFCore.BulkExtensions](https://github.com/borisdj/EFCore.BulkExtensions) ...支持efcore的扩展生态
- [提供多种默认分表规则路由] 按时间,按取模 可自定义
- [针对分页进行优化] 大页数跳转支持低内存流式处理，高性能分页

## 缺点
- [消耗连接]出现分表与分表对象进行join如果条件没法索引到具体表会生成```笛卡尔积```导致连接数爆炸,后期会进行针对该情况的配置

## 安装
```xml
<PackageReference Include="ShardingCore" Version="5.LastVersion" />
or
<PackageReference Include="ShardingCore" Version="3.LastVersion" />
or
<PackageReference Include="ShardingCore" Version="2.LastVersion" />
```

# 开始
## 分表

我们以用户取模来做例子,配置entity 推荐 [fluent api](https://docs.microsoft.com/en-us/ef/core/modeling/) 
`IShardingTable`数据库对象必须继承该接口
`ShardingTableKey`分表字段需要使用该特性

```c#
    public class SysUserMod : IShardingTable
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
        //2 tail length:00,01,02......99
        //3 hashcode % 3: [0,1,2]
        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }
    }
```

如果你使用分表必须创建一个继承自```IShardingTableDbContext```接口的DbContext,
必须实现```IShardingDbContext```,默认提供了AbstractShardingDbContext

```c#

  //DefaultTableDbContext is acutal execute dbcontext
    public class DefaultShardingDbContext:AbstractShardingDbContext,IShardingTableDbContext
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
        }
        public IRouteTail RouteTail { get; set; }

    }
```
`Startup.cs` 下的 `ConfigureServices(IServiceCollection services)`

```c#

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //if u want use no sharding operate
            //services.AddDbContext<DefaultTableDbContext>(o => o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True"));

    //add shardingdbcontext support life scope
                
        services.AddShardingDbContext<DefaultShardingDbContext>(
                   (conStr, builder) => builder.UseSqlServer(conStr)
                )
                .Begin(o =>
                {
                    o.CreateShardingTableOnStart = true;//create sharding table
                    o.EnsureCreatedWithOutShardingTable = true;//create data source with out sharding table
                })  .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0", "Data Source=localhost;Initial Catalog=ShardingCoreDB1;Integrated Security=True;")
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                }).End();
```

`Startup.cs` 下的 ` Configure(IApplicationBuilder app, IWebHostEnvironment env)` 你也可以自行封装[app.UseShardingCore()](https://github.com/xuejmnet/sharding-core/blob/main/samples/Sample.SqlServer/DIExtension.cs)

```c#

            var shardingBootstrapper = app.ApplicationServices.GetRequiredService<IShardingBootstrapper>();
            shardingBootstrapper.Start();
```
如何使用
```c#
    
        private readonly DefaultShardingDbContext _defaultShardingDbContext;

        public ctor(DefaultShardingDbContext defaultShardingDbContext)
        {
            _defaultShardingDbContext = defaultShardingDbContext;
        }

        public async Task Insert_1000()
        {
            if (!_defaultShardingDbContext.Set<SysUserMod>().Any())
                {
                    var ids = Enumerable.Range(1, 1000);
                    var userMods = new List<SysUserMod>();
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_{id}",
                            AgeGroup = Math.Abs(id % 10)
                        });
                    }

                    _defaultShardingDbContext.AddRange(userMods);

                   await _defaultShardingDbContext.SaveChangesAsync();
                }
        }
        public async Task ToList_All()
        {
            
            var mods = await _defaultShardingDbContext.Set<SysUserMod>().ToListAsync();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = await _defaultShardingDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToListAsync();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }

            var modOrders2 = await _defaultShardingDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToListAsync();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }
        }
```
## 分库

我们还是以用户取模来做例子,配置entity 推荐 [fluent api](https://docs.microsoft.com/en-us/ef/core/modeling/) 
`IShardingDataSource`数据库对象必须继承该接口
`ShardingDataSourceKey`分库字段需要使用该特性

```c#
    public class SysUserMod : IShardingDataSource
    {
        /// <summary>
        /// 用户Id用于分库
        /// </summary>
        [ShardingDataSourceKey]
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

    
    public class SysUserModVirtualDataSourceRoute:AbstractShardingOperatorVirtualDataSourceRoute<SysUserMod,string>
    {
        protected readonly int Mod=3;
        protected readonly int TailLength=1;
        protected readonly char PaddingChar='0';

        protected override string ConvertToShardingKey(object shardingKey)
        {
            return shardingKey.ToString();
        }

        public override string ShardingKeyToDataSourceName(object shardingKey)
        {
            var shardingKeyStr = ConvertToShardingKey(shardingKey);
            return "ds"+Math.Abs(ShardingCoreHelper.GetStringHashCode(shardingKeyStr) % Mod).ToString().PadLeft(TailLength, PaddingChar); ;
        }

        public override List<string> GetAllDataSourceNames()
        {
            return new List<string>()
            {
                "ds0",
                "ds1",
                "ds2"
            };
        }

        public override bool AddDataSourceName(string dataSourceName)
        {
            throw new NotImplementedException();
        }

        protected override Expression<Func<string, bool>> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator)
        {

            var t = ShardingKeyToDataSourceName(shardingKey);
            switch (shardingOperator)
            {
                case ShardingOperatorEnum.Equal: return tail => tail == t;
                default:
                {
                    return tail => true;
                }
            }
        }
    }
```

如果你使用分库就不需要```IShardingTableDbContext```接口的DbContext
创建分表DbContext必须继承AbstractShardingDbContext


```c#

  //DefaultTableDbContext is acutal execute dbcontext
    public class DefaultShardingDbContext:AbstractShardingDbContext
    {
        public DefaultShardingDbContext(DbContextOptions<DefaultShardingDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SysUserModMap());
        }

    }
```
`Startup.cs` 下的 `ConfigureServices(IServiceCollection services)`

```c#

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

    //add shardingdbcontext support life scope
                
       services.AddShardingDbContext<DefaultShardingDbContext>(
                   (conStr, builder) => builder.UseSqlServer(conStr)
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                })
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0","Data Source=localhost;Initial Catalog=ShardingCoreDBxx0;Integrated Security=True;")
                .AddShardingDataSource(sp =>
                {
                    return new Dictionary<string, string>()
                    {
                        {"ds1", "Data Source=localhost;Initial Catalog=ShardingCoreDBxx1;Integrated Security=True;"},
                        {"ds2", "Data Source=localhost;Initial Catalog=ShardingCoreDBxx2;Integrated Security=True;"},
                    };
                }).AddShardingDataSourceRoute(o =>
                {
                    o.AddShardingDatabaseRoute<SysUserModVirtualDataSourceRoute>();
                }).End();
```

`Startup.cs` 下的 ` Configure(IApplicationBuilder app, IWebHostEnvironment env)` 你也可以自行封装[app.UseShardingCore()](https://github.com/xuejmnet/sharding-core/blob/main/samples/Sample.SqlServer/DIExtension.cs)

```c#

            var shardingBootstrapper = app.ApplicationServices.GetRequiredService<IShardingBootstrapper>();
            shardingBootstrapper.Start();
```
如何使用
```c#
    
        private readonly DefaultShardingDbContext _defaultShardingDbContext;

        public ctor(DefaultShardingDbContext defaultShardingDbContext)
        {
            _defaultShardingDbContext = defaultShardingDbContext;
        }

        public async Task Insert_1000()
        {
            if (!_defaultShardingDbContext.Set<SysUserMod>().Any())
                {
                    var ids = Enumerable.Range(1, 1000);
                    var userMods = new List<SysUserMod>();
                    foreach (var id in ids)
                    {
                        userMods.Add(new SysUserMod()
                        {
                            Id = id.ToString(),
                            Age = id,
                            Name = $"name_{id}",
                            AgeGroup = Math.Abs(id % 10)
                        });
                    }

                    _defaultShardingDbContext.AddRange(userMods);

                   await _defaultShardingDbContext.SaveChangesAsync();
                }
        }
        public async Task ToList_All()
        {
            
            var mods = await _defaultShardingDbContext.Set<SysUserMod>().ToListAsync();
            Assert.Equal(1000, mods.Count);

            var modOrders1 = await _defaultShardingDbContext.Set<SysUserMod>().OrderBy(o => o.Age).ToListAsync();
            int ascAge = 1;
            foreach (var sysUserMod in modOrders1)
            {
                Assert.Equal(ascAge, sysUserMod.Age);
                ascAge++;
            }

            var modOrders2 = await _defaultShardingDbContext.Set<SysUserMod>().OrderByDescending(o => o.Age).ToListAsync();
            int descAge = 1000;
            foreach (var sysUserMod in modOrders2)
            {
                Assert.Equal(descAge, sysUserMod.Age);
                descAge--;
            }
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
分库提供了默认的路由分表则需要自己去实现,具体实现可以参考分库

抽象abstract | 路由规则 | tail | 索引
--- |--- |--- |--- 
AbstractSimpleShardingModKeyIntVirtualTableRoute |取模 |0,1,2... | `=,contains`
AbstractSimpleShardingModKeyStringVirtualTableRoute |取模 |0,1,2... | `=,contains`
AbstractSimpleShardingDayKeyDateTimeVirtualTableRoute |按时间 |yyyyMMdd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingDayKeyLongVirtualTableRoute |按时间戳 |yyyyMMdd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingWeekKeyDateTimeVirtualTableRoute |按时间 |yyyyMMdd_dd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingWeekKeyLongVirtualTableRoute |按时间戳 |yyyyMMdd_dd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingMonthKeyDateTimeVirtualTableRoute |按时间 |yyyyMM | `>,>=,<,<=,=,contains`
AbstractSimpleShardingMonthKeyLongVirtualTableRoute |按时间戳 |yyyyMM | `>,>=,<,<=,=,contains`
AbstractSimpleShardingYearKeyDateTimeVirtualTableRoute |按时间 |yyyy | `>,>=,<,<=,=,contains`
AbstractSimpleShardingYearKeyLongVirtualTableRoute |按时间戳 |yyyy | `>,>=,<,<=,=,contains`

注:`contains`表示为`o=>ids.contains(o.shardingkey)`
注:使用默认的按时间分表的路由规则会让你重写一个GetBeginTime的方法这个方法必须使用静态值如:new DateTime(2021,1,1)不可以用动态值比如DateTime.Now因为每次重新启动都会调用该方法动态情况下会导致每次都不一致

# 高级

## code-first
目前`sharding-core`已经支持code first支持代码现行，具体实现可以参考[Migrations](https://github.com/xuejmnet/sharding-core/tree/main/samples/Sample.Migrations/readme.md)

## 自动追踪
默认shardingcore不支持自动追踪,并且也不建议使用自动追踪,如果你有需要shardingcore也默认提供了自动追踪功能
有两点需要注意
目前仅支持单主键对象
1.shardingcore仅支持dbcontext的model的类型的整个查询匿名类型不支持联级查询不支持
2.shardingcore的单个查询依然走数据库不走缓存如果查询出来的结果缓存里面有就返回缓存里面的而不是数据库的
3.tolist等操作会查询数据库返回的时候判断是否已经追踪如果已经追踪则返回缓存里已经追踪了的值
4.支持 `first`,`firstordefault`,`last`,`lastordefault`,`single`,`singleordefault`
如何开启
```c#
services.AddShardingDbContext<DefaultShardingDbContext>(.......)
            .Begin(o => {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                    //autotrack support asnotracking astracking QueryTrackingBehavior.TrackAll
                    o.AutoTrackEntity = true; 
                })
```

## 手动路由
```c#
ctor inject IShardingRouteManager shardingRouteManager

    public class SysUserModVirtualTableRoute : AbstractSimpleShardingModKeyStringVirtualTableRoute<SysUserMod>
    {
        /// <summary>
        /// 开启提示路由
        /// </summary>
        protected override bool EnableHintRoute => true;

        public SysUserModVirtualTableRoute() : base(2,3)
        {
        }
    }


    

    using (_shardingRouteManager.CreateScope())
    {
        _shardingRouteManager.Current.TryCreateOrAddMustTail<SysUserMod>("00");

        var mod00s = await _defaultTableDbContext.Set<SysUserMod>().Skip(10).Take(11).ToListAsync();
    }
```

## 自动建表
[参考](https://github.com/xuejmnet/sharding-core/tree/main/samples/Samples.AutoByDate.SqlServer)

## 事务
1.默认savechanges支持事务
```c#

 await  _defaultShardingDbContext.SaveChangesAsync();
     
```
2.手动开启事务 [请参考微软](https://docs.microsoft.com/zh-cn/ef/core/saving/transactions)
```c#
            using (var tran = _defaultTableDbContext.DataBase.BeginTransaction())
            {
                    ........
                _defaultTableDbContext.SaveChanges();
                tran.Commit();
            }
```


## 批量操作

批量操作将对应的dbcontext和数据进行分离由用户自己选择第三方框架比如[`Z.EntityFramework.Plus.EFCore`](https://github.com/zzzprojects/EntityFramework-Plus) 进行批量操作或者 [`EFCore.BulkExtensions`](https://github.com/borisdj/EFCore.BulkExtensions) ,支持一切三方批量框架
```c#
var list = new List<SysUserMod>();
///通过集合返回出对应的k-v归集通过事务开启
            var dbContexts = _defaultTableDbContext.BulkShardingEnumerable(list);

           
                    foreach (var dataSourceMap in dbContexts)
                    {
                        foreach (var tailMap in dataSourceMap.Value)
                        {
                            tailMap.Key.BulkInsert(tailMap.Value.ToList());
                            //tailMap.Key.BulkDelete(tailMap.Value.ToList());
                            //tailMap.Key.BulkUpdate(tailMap.Value.ToList());
                        }
                    }
                _defaultTableDbContext.SaveChanges();
          //or
            var dbContexts = _defaultTableDbContext.BulkShardingEnumerable(list);
            using (var tran = _defaultTableDbContext.Database.BeginTransaction())
            {
                    foreach (var dataSourceMap in dbContexts)
                    {
                        foreach (var tailMap in dataSourceMap.Value)
                        {
                            tailMap.Key.BulkInsert(tailMap.Value.ToList());
                            //tailMap.Key.BulkDelete(tailMap.Value.ToList());
                            //tailMap.Key.BulkUpdate(tailMap.Value.ToList());
                        }
                    }
                _defaultTableDbContext.SaveChanges();
                tran.Commit();
            }

```
## code-first

## 读写分离
该框架目前已经支持一主多从的读写分离`AddReadWriteSeparation`,支持轮询 Loop和随机 Random两种读写分离策略,又因为读写分离多链接的时候会导致数据读写不一致,(如分页其实是2步第一步获取count，第二部获取list)会导致数据量在最后几页出现缺量的问题,
针对这个问题框架目前实现了自定义读链接获取策略`ReadConnStringGetStrategyEnum.LatestEveryTime`表示为每次都是新的(这个情况下会出现上述问题),`ReadConnStringGetStrategyEnum.LatestFirstTime`表示以dbcontext作为单位获取一次(同dbcontext不会出现问题),
又因为各节点读写分离网络等一系列问题会导致刚刚写入的数据没办法获取到所以系统默认在dbcontext上添加是否使用读写分离如果false默认选择写字符串去读取`_defaultTableDbContext.ReadWriteSeparation=false`

```c#
services.AddShardingDbContext<DefaultShardingDbContext>(
                    (conStr, builder) => builder.UseSqlServer(conStr).UseLoggerFactory(efLogger)
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = true;
                    o.EnsureCreatedWithOutShardingTable = true;
                })
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection).UseLoggerFactory(efLogger))
                .AddDefaultDataSource("ds0",
                    "Data Source=localhost;Initial Catalog=ShardingCoreDB1;Integrated Security=True;")
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<SysUserModVirtualTableRoute>();
                }).AddReadWriteSeparation(o =>
                {
                    return new Dictionary<string, ISet<string>>()
                    {
                        {
                            "ds0", new HashSet<string>(){
                            "Data Source=localhost;Initial Catalog=ShardingCoreDBReadOnly1;Integrated Security=True;",
                            "Data Source=localhost;Initial Catalog=ShardingCoreDBReadOnly2;Integrated Security=True;"}
                        }
                    };
                }, ReadStrategyEnum.Loop).End();

                //reslove read write delay data not found
                _defaultTableDbContext.ReadWriteSeparation=false;
                //dbcontext use write connection string 
```

## 高性能分页
sharding-core本身使用流式处理获取数据在普通情况下和单表的差距基本没有,但是在分页跳过X页后,性能会随着X的增大而减小O(n)
目前该框架已经实现了一套高性能分页可以根据用户配置,实现分页功能。

支持版本`x.2.0.16+`

1.如何开启分页配置 比如我们针对用户月新表进行分页配置,先实现`IPaginationConfiguration<>`接口,该接口是分页配置接口
```c#

    public class SysUserSalaryPaginationConfiguration:IPaginationConfiguration<SysUserSalary>
    {
        public void Configure(PaginationBuilder<SysUserSalary> builder)
        {
            builder.PaginationSequence(o => o.Id)
                .UseRouteCompare(Comparer<string>.Default)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch);
            builder.PaginationSequence(o => o.DateOfMonth)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone(10);
            builder.PaginationSequence(o => o.Salary)
                .UseQueryMatch(PaginationMatchEnum.Owner | PaginationMatchEnum.Named | PaginationMatchEnum.PrimaryMatch).UseAppendIfOrderNone();
            builder.ConfigReverseShardingPage(0.5d,10000L);
        }
    }
```
2.添加配置
在对应的用户路由中添加配置 [XXXXXXVirtualTableRoute]
```c#
        public override IPaginationConfiguration<SysUserSalary> CreatePaginationConfiguration()
        {
            return new SysUserSalaryPaginationConfiguration();
        }
```
3.Configure内部为什么意思?
1) builder.PaginationSequence(o => o.Id) 配置当分页orderby 字段为Id时那么分表所对应的表结构为顺序,顺序的规则通过`UseRouteCompare`来设置,其中string为表tail 或 data source name,
具体什么意思就是说如果本次分页设计3张表分别是table1,table2,table3,如果我没配置id的情况下那么需要查询3张表然后分别进行流式聚合,如果我配置了id的情况下,如果本次sql查询带上了id作为order by字段
   那么就不需要分别查询3张表,可以直接查询table1如果table1的count大于你要跳过的页数,假设分页查询先查询多少条,table1:100条,table2:200条,table3:300条
   如果你要跳过90条获取10条原先的时间就是O(100)现在的时间就是O(10)因为table1跳过了90条还剩余10条;
2) `UseQueryMatch`是什么意思,这个就是表示你要匹配的规则,是必须是当前这个类下的属性还是说只需要排序名称一样即可,因为有可能select new{}匿名对象类型就会不一样,`PrimaryMatch`表示是否只需要第一个主要的
orderby匹配上就行了,`UseAppendIfOrderNone`表示是否需要开启在没有对应order查询条件的前提下添加本属性排序,这样可以保证顺序排序性能最优
3) `builder.ConfigReverseShardingPage` 表示是否需要启用反向排序,因为正向排序在skip过多后会导致需要跳过的数据过多,尤其是最后几页,如果开启其实最后几页就是前几页的反向排序,其中第一个参数表示跳过的因子,就是说
skip必须大于分页总total*该因子(0-1的double),第二个参数表示最少需要total多少条必须同时满足两个条件才会开启(必须大于500),并且反向排序优先级低于顺序排序,
4.如何使用
 ```c#
var shardingPageResultAsync = await _defaultTableDbContext.Set<SysUserMod>().OrderBy(o=>o.Age).ToShardingPageAsync(pageIndex, pageSize);
```
### 注意:如果你是按时间排序无论何种排序建议开启并且加上时间顺序排序,如果你是取模或者自定义分表,建议将Id作为顺序排序,如果没有特殊情况请使用id排序并且加上反向排序作为性能优化,如果entity同时支持分表分库并且两个路由都支持同一个属性的顺序排序优先级为先分库后分表


# 注意事项
使用该框架需要注意两点如果你的shardingdbcontext重写了以下服务可能无法使用 如果还想使用需要自己重写扩展[请参考](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/DIExtension.cs)
1.shardingdbcontext
```c#
   return optionsBuilder.UseShardingWrapMark()
                .ReplaceService<IDbSetSource, ShardingDbSetSource>()
                .ReplaceService<IQueryCompiler, ShardingQueryCompiler>()
                .ReplaceService<IDbContextTransactionManager, ShardingRelationalTransactionManager<TShardingDbContext>>()
                .ReplaceService<IRelationalTransactionFactory, ShardingRelationalTransactionFactory<TShardingDbContext>>();
```
2.defaultdbcontext
```c#
return optionsBuilder.ReplaceService<IModelCacheKeyFactory, ShardingModelCacheKeyFactory>()
                .ReplaceService<IModelCustomizer, ShardingModelCustomizer<TShardingDbContext>>();

```
,目前框架采用AppDomain.CurrentDomain.GetAssemblies();
可能会导致程序集未被加载所以尽可能在api层加载所需要的dll
使用时需要注意
- 分表实体对象是否继承`IShardingTable`
- 分表实体对象是否有`ShardingKey`
- 分库实体对象是否继承`IShardingDataSource`
- 分库实体对象是否有`ShardingDataSourceKey`
- 实体对象是否已经实现了一个虚拟路由
- startup是否已经添加虚拟路由
- startup是否已经添加bootstrapper.start()

```c#
//支持最终修改
            var sresult =  _defaultTableDbContext.Set<SysUserMod>().ToList();

            var sysUserMod98 = result.FirstOrDefault(o => o.Id == "98");
            sysUserMod98.Name = "name_update"+new Random().Next(1,99)+"_98";
            await _defaultTableDbContext.SaveChangesAsync();
--log info
  Executed DbCommand (1ms) [Parameters=[@p1='?' (Size = 128), @p0='?' (Size = 128)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      UPDATE [SysUserMod_02] SET [Name] = @p0
      WHERE [Id] = @p1;
      SELECT @@ROWCOUNT;
```


# 计划
- [提供官网如果该项目比较成功的话]
- [开发更完善的文档]
- [重构成支持.net其他orm]

# 最后
该框架借鉴了大部分分表组件的思路,目前提供的接口都已经实现,并且支持跨表查询,基于分页查询该框架也使用了流式查询保证不会再skip大数据的时候内存会爆炸,目前这个库只是一个刚刚成型的库还有很多不完善的地方希望大家多多包涵,如果喜欢的话也希望大家给个star.
该文档是我晚上赶工赶出来的也想趁热打铁希望更多的人关注,也希望更多的人可以交流。

凭借各大开源生态圈提供的优秀代码和思路才有的这个框架,希望可以为.Net生态提供一份微薄之力,该框架本人会一直长期维护,有大神技术支持可以联系下方方式欢迎star :)

[博客](https://www.cnblogs.com/xuejiaming)

QQ群:771630778

个人QQ:326308290(欢迎技术支持提供您宝贵的意见)

个人邮箱:326308290@qq.com