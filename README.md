<h1 align="center"> ShardingCore </h1>

`ShardingCore` 是一个支持efcore 2.x 3.x 5.x的一个对于数据库分表的一个简易扩展,
目前该库暂未支持分库(未来会支持),仅支持分表,该项目的理念是让你可以已最少的代码量来实现自动分表的实现,经过多个开源项目的摸索参考目前正式开源本项目

### 依赖

Release  | EF Core | .NET Standard | .NET (Core) | Sql Server | Pomelo.EntityFrameworkCore.MySql
--- | --- | --- | --- | --- | --- 
[5.x.x.x](https://www.nuget.org/packages/ShardingCore/5.0.0.1) | >= 5.0.x | 2.1 | 3.0+ | >= 2012 | 5.0.0-alpha.2
[3.x.x.x](https://www.nuget.org/packages/ShardingCore/3.0.0.1) | 3.1.10 | 2.0 | 2.0+ | >= 2012 |  3.2.4
[2.x.x.x](https://www.nuget.org/packages/ShardingCore/2.0.0.1) | 2.2.6 | 2.0 | 2.0+ | >= 2008 |  2.2.6

### Support Sharding Method 支持的分表方式

Support
Any
Method
and
Support
Any
ShardingKey
not
provide
job
but
provider
create
table
interface [IShardingTableCreator](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/TableCreator/IShardingTableCreator.cs)
simple
job [ChronusJob](https://github.com/xuejmnet/ChronusJob)
support
cron
expression

方法  |Method  | Support | ShardingKey Type
--- |--- | --- | --- 
取模 |Sharding Mod | Yes | Any ClrType
大数取模范围 |Sharding Range | Yes | Any ClrType
按天/周/月/年... |Sharding By Day/Week/Month/Year... | Yes | Any ClrType
其他 |Sharding By Other | Yes | Any ClrType

- [开始](#开始)
    - [概念](#概念)
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

以下所有例子都以Sql
Server为例

## 概念

本库的几个简单的核心概念:

- [Tail]
  尾巴、后缀物理表的后缀
- [TailPrefix]
  尾巴前缀虚拟表和物理表的后缀中间的字符
- [物理表]
  顾名思义就是数据库对应的实际表信息,表名(
  tablename
  +
  tailprefix
  +
  tail) [IPhysicTable](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/PhysicTables/IPhysicTable.cs)
- [虚拟表]
  虚拟表就是系统将所有的物理表在系统里面进行抽象的一个总表对应到程序就是一个entity[IVirtualTable](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualTables/IVirtualTable.cs)
- [虚拟路由]
  虚拟路由就是联系虚拟表和物理表的中间介质,虚拟表在整个程序中只有一份,那么程序如何知道要查询系统哪一张表呢,最简单的方式就是通过虚拟表对应的路由[IVirtualRoute](https://github.com/xuejmnet/sharding-core/blob/main/src/ShardingCore/Core/VirtualRoutes/IVirtualRoute.cs)
  ,由于基本上所有的路由都是和业务逻辑相关的所以虚拟路由由用户自己实现,该框架提供一个高级抽象

## 安装
```xml
<PackageReference Include="ShardingCore.SqlServer" Version="5.0.0.1" />
```

## 配置

配置entity 推荐 [fluent api](https://docs.microsoft.com/en-us/ef/core/modeling/) 可以实现自动建表功能
`IShardingEntity`数据库对象必须继承该接口
`ShardingKey`分表字段需要使用该特性

```c#

    public class SysUserRange:IShardingEntity
    {
        /// <summary>
        /// 分表分库range切分
        /// </summary>
        [ShardingKey(TailPrefix = "_",AutoCreateTableOnStart = true)]
        public string Id { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int Age { get; set; }
    }
    
    public class SysUserRangeMap:IEntityTypeConfiguration<SysUserRange>
    {
        public void Configure(EntityTypeBuilder<SysUserRange> builder)
        {
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Id).IsRequired().HasMaxLength(128);
            builder.Property(o => o.Name).HasMaxLength(128);
            builder.ToTable(nameof(SysUserRange));
        }
    }
    
```
创建virtual
route
实现 `AbstractShardingOperatorVirtualRoute<T, TKey>`
抽象
框架默认有提供几个简单的路由 [默认路由](#默认路由)

```c#

    public class SysUserRangeVirtualRoute: AbstractShardingOperatorVirtualRoute<SysUserRange, string>
    {
        protected override string ConvertToShardingKey(object shardingKey);

        public override string ShardingKeyToTail(object shardingKey);

        public override List<string> GetAllTails();

        protected override Expression<Func<string, bool>> GetRouteToFilter(string shardingKey, ShardingOperatorEnum shardingOperator);
     }
```

- `ConvertToShardingKey`
  分表关键字段如何转换成对应的类型
- `ShardingKeyToTail`
  分表关键字段如何转换成对应的物理表后缀
- `GetAllTails`
  现在数据库已存在的尾巴有哪些
- `GetRouteToFilter`
  传入分表字段返回一个如何筛选尾巴的方法

`Startup.cs` 下的 `ConfigureServices(IServiceCollection services)`

```c#

  services.AddShardingSqlServer(o =>
  {
      o.ConnectionString = "Data Source=localhost;Initial Catalog=ShardingCoreDB;Integrated Security=True";
      o.AddSharding<SysUserRangeVirtualRoute>();
      o.CreateIfNotExists((provider, config) =>
      {
          config.EnsureCreated = true;
      });
  });
```

`Startup.cs` 下的 ` Configure(IApplicationBuilder app, IWebHostEnvironment env)` 你也可以自行封装[app.UseShardingCore()](https://github.com/xuejmnet/sharding-core/blob/main/samples/Sample.SqlServer/DIExtension.cs)

```c#

            var shardingBootstrapper = app.ApplicationServices.GetRequiredService<IShardingBootstrapper>();
            shardingBootstrapper.Start();
```

## 使用
```c#
    
        private readonly IVirtualDbContext _virtualDbContext;

        public ctor(IVirtualDbContext virtualDbContext)
        {
            _virtualDbContext = virtualDbContext;
        }

        public async Task ToList_All()
        {
            var ranges=await _virtualDbContext.Set<SysUserRange>().ToShardingListAsync();
        }
```

## Api

方法  | Method | SqlServer Unit Test | MySql Unit Test
--- |--- |--- |---
获取集合 |ToShardingListAsync |8 |8
第一条 |ShardingFirstOrDefaultAsync |5 |5
最大 |ShardingMaxAsync |0 |0
最小 |ShardingMinAsync |0 |0
是否存在 |ShardingAnyAsync |0 |0
分页 |ToShardingPageResultAsync |0 |0
数目 |ShardingCountAsync |0 |0
求和 |ShardingSumAsync |0 |0

## 默认路由

抽象abstract  | 路由规则 | tail | 索引
--- |--- |--- |--- 
AbstractSimpleShardingModKeyIntVirtualRoute |默认int类型取模路由 |0,1,2... | `=`
AbstractSimpleShardingModKeyStringVirtualRoute |默认string类型取模路由 |0,1,2... | `=`
AbstractSimpleShardingDayKeyDateTimeVirtualRoute |默认DateTime类型取模路由 |yyyyMMdd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingDayKeyLongVirtualRoute |默认long类型取模路由 |yyyyMMdd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingWeekKeyDateTimeVirtualRoute |默认DateTime类型取模路由 |yyyyMMdd_dd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingWeekKeyLongVirtualRoute |默认long类型取模路由 |yyyyMMdd_dd | `>,>=,<,<=,=,contains`
AbstractSimpleShardingMonthKeyDateTimeVirtualRoute |默认DateTime类型取模路由 |yyyyMM | `>,>=,<,<=,=,contains`
AbstractSimpleShardingMonthKeyLongVirtualRoute |默认long类型取模路由 |yyyyMM | `>,>=,<,<=,=,contains`
AbstractSimpleShardingYearKeyDateTimeVirtualRoute |默认DateTime类型取模路由 |yyyy | `>,>=,<,<=,=,contains`
AbstractSimpleShardingYearKeyLongVirtualRoute |默认long类型取模路由 |yyyy | `>,>=,<,<=,=,contains`

注:`contains`表示为`o=>ids.contains(o.shardingkey)`

# 最后
凭借各大开源生态圈提供的优秀代码和思路才有的这个框架,希望可以为.Net生态提供一份微薄之力,该框架本人会一直长期维护,有大神技术支持可以联系下方方式欢迎star :)

QQ群:771630778

个人QQ:326308290(欢迎技术支持提供您宝贵的意见)

个人邮箱:326308290@qq.com