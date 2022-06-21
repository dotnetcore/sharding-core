// 解决 PostgreSQL 在.NET 6.0 使用 DateTime 类型抛出异常：timestamp with time zone
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);       // 启用旧时间戳行为
AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);  // 禁用日期时间无限转换

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
