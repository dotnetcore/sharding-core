本项目迁移参考[efcore.sharding](https://github.com/Coldairarrow/EFCore.Sharding/tree/master/examples/Demo.DbMigrator) 后续会不断完善
## 迁移步骤

    1. 执行迁移命令，但不执行更新命令
    2. 执行脚本命令，获得到Sql语句后，在开发环境的数据库中执行

#### 迁移数据库

	初始化：Add-Migration InitialCreate (Add-Migration EFCoreSharding -Context DefaultShardingTableDbContext -OutputDir Migrations\ShardingMigrations)
	迁移：Add-Migration [NewMigration] -Context CustomContext
	还原：Remove-Migration -Context CustomContext
	脚本：Script-Migration -Context CustomContext -from:[TargetMigration] ( Script-Migration -Context DefaultShardingTableDbContext)用于生产环境
	更新：Update-Database -Context CustomContext （Update-Database -Context DefaultShardingTableDbContext -Verbose）

## 迁移说明

	Scaffold-DbContext 'Data Source=.;Initial Catalog=TesbDb;Integrated Security=True;Pooling=true;' -Schemas 'PDEV' -UseDatabaseNames -DataAnnotations -Force Microsoft.EntityFrameworkCore.SqlServer

1、获取帮助信息

    Get-Help about_EntityFrameworkCore

2.1、一般的初始化数据库指令（将会创建新数据库）：

	Add-Migration InitialCreate

	效果：会在项目中生成类似 201708220135292_InitialCreate.cs 的类，里面包含了数据库的创建逻辑。

2.2、已存在数据库时的初始化指令（只生成__MigrationHistory表）：

	Add-Migration InitialCreate –IgnoreChanges
	
	效果：会在项目中生成类似 201708220135292_InitialCreate.cs 的类，里面不包含任何逻辑。

3、添加新的迁移

	Add-Migration [NewMigration]

	效果：会在项目中生成类似 201708220136184_AddSomething.cs 的类，里面包含了本次迁移的更改逻辑。
	注意：AddSomething 的命名是唯一的，所以要注意妥善命名。

4、删除迁移

    Remove-Migration

5.1、更新数据库

	Update-Database

	效果：将未应用的迁移提交到数据库。
	
5.2、更新数据库（-Verbose）

	Update-Database -Verbose

	效果：同时显示执行的SQL语句。

5.3、更新数据库（-TargetMigration）

	Update-Database -TargetMigration:AddSomething

	效果：这个命令将会运行 AddSomething 之后的所有迁移的 Down 命令，从而将数据库还原到 AddSomething 的版本。
	特别地：Update-Database –TargetMigration: $InitialDatabase 可以将数据库还原到最初始的版本。

6、得到SQL脚本（-Script），常用于将更改发布到生产环境

	Script-Migration
    例子 获取从InitialCreate 到 AddInspectorWorkShift的变更的sql脚本
    Script-Migration -Context BusinessDbContext -from InitialCreate -to AddInspectorWorkShift

    此命令有几个选项。
    from 迁移应是运行该脚本前应用到数据库的最后一个迁移。 如果未应用任何迁移，请指定 0（默认值）。
    to 迁移是运行该脚本后应用到数据库的最后一个迁移。 它默认为项目中的最后一个迁移。
    可以选择生成 idempotent 脚本。 此脚本仅会应用尚未应用到数据库的迁移。 如果不确知应用到数据库的最后一个迁移或需要部署到多个可能分别处于不同迁移的数据库，此脚本非常有用。


##注意点
```c#
//创建对应的IDesignTimeDbContextFactory并且设置启动时不建表启动时不建库 `.ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator<DefaultShardingTableDbContext>>()` 添加自动分表迁移
    public class DefaultDesignTimeDbContextFactory: IDesignTimeDbContextFactory<DefaultShardingTableDbContext>
    { 
        static DefaultDesignTimeDbContextFactory()
        {
            var services = new ServiceCollection();
            services.AddShardingDbContext<DefaultShardingTableDbContext, DefaultTableDbContext>(
                    o =>
                        o.UseSqlServer("Data Source=localhost;Initial Catalog=ShardingCoreDBMigration;Integrated Security=True;")
                            .ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator<DefaultShardingTableDbContext>>()
                ).Begin(o =>
                {
                    o.CreateShardingTableOnStart = false;
                    o.EnsureCreatedWithOutShardingTable = false;
                    o.AutoTrackEntity = true;
                })
                .AddShardingQuery((conStr, builder) => builder.UseSqlServer(conStr)
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking))
                .AddShardingTransaction((connection, builder) =>
                    builder.UseSqlServer(connection))
                .AddDefaultDataSource("ds0",
                    "Data Source=localhost;Initial Catalog=ShardingCoreDBMigration;Integrated Security=True;")
                .AddShardingTableRoute(o =>
                {
                    o.AddShardingTableRoute<ShardingWithModVirtualTableRoute>();
                    o.AddShardingTableRoute<ShardingWithDateTimeVirtualTableRoute>();
                }).End();
            services.AddLogging();
            var buildServiceProvider = services.BuildServiceProvider();
            ShardingContainer.SetServices(buildServiceProvider);
            new ShardingBootstrapper(buildServiceProvider).Start();
        }
        
        public DefaultShardingTableDbContext CreateDbContext(string[] args)
        {
            return ShardingContainer.GetService<DefaultShardingTableDbContext>();
        }
    }
```