// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Linq;
// using Microsoft.EntityFrameworkCore;
// using ShardingCore.Extensions;
// using ShardingCore.Helpers;
//
// namespace ShardingCore.DbContexts.ShardingDbContexts
// {
// /*
// * @Author: xjm
// * @Description:
// * @Date: Wednesday, 16 December 2020 15:28:12
// * @Email: 326308290@qq.com
// */
//     public class ShardingDbContext : DbContext
//     {
//         public string Tail { get; }
//         public List<VirtualTableDbContextConfig> VirtualTableConfigs { get; }
//         public bool RemoveRemoveShardingEntity { get; }
//         private static readonly ConcurrentDictionary<Type, Type> _entityTypeConfigurationTypeCaches = new ConcurrentDictionary<Type, Type>();
//         private static readonly object buildEntityTypeConfigurationLock = new object();
//
//         public ShardingDbContext(ShardingDbContextOptions shardingDbContextOptions) : base(shardingDbContextOptions.DbContextOptions)
//         {
//             Tail = shardingDbContextOptions.Tail;
//             VirtualTableConfigs = shardingDbContextOptions.VirtualTableDbContextConfigs;
//             RemoveRemoveShardingEntity = shardingDbContextOptions.RemoveShardingEntity;
//         }
//
//         /// <summary>
//         /// 模型构建
//         /// </summary>
//         /// <param name="modelBuilder"></param>
//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             if (!string.IsNullOrWhiteSpace(Tail))
//             {
//                 //支持IEntityTypeConfiguration配置
//                 VirtualTableConfigs.ForEach(virtualTable =>
//                 {
//                     var shardingEntityType = virtualTable.ShardingEntityType;
//                     if (!_entityTypeConfigurationTypeCaches.TryGetValue(shardingEntityType, out var entityTypeConfigurationType))
//                         throw new Exception($"未找到对应的类型无法进行IEntityTypeConfiguration配置:[{shardingEntityType.Name}]");
//                     if (entityTypeConfigurationType == null)
//                         throw new NotSupportedException($"{shardingEntityType}的[IBaseEntityTypeConfiguration]未找到");
//                     var method = modelBuilder.GetType()
//                         .GetMethods()
//                         .FirstOrDefault(x => x.Name == nameof(ModelBuilder.ApplyConfiguration)
//                                              && x.GetParameters().Count() == 1
//                                              && x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
//                     method.MakeGenericMethod(shardingEntityType).Invoke(modelBuilder, new object[] {Activator.CreateInstance(entityTypeConfigurationType)});
//                 });
//
//                 VirtualTableConfigs.ForEach(virtualTableConfig =>
//                 {
//                     var shardingEntity = virtualTableConfig.ShardingEntityType;
//                     var tailPrefix = virtualTableConfig.TailPrefix;
//                     var entity = modelBuilder.Entity(shardingEntity);
//                     var tableName = virtualTableConfig.OriginalTableName;
//                     if (string.IsNullOrWhiteSpace(tableName))
//                         throw new ArgumentNullException($"{shardingEntity}:无法找到对应的原始表名。");
// #if DEBUG
//                     Console.WriteLine($"映射表:[tableName]-->[{tableName}{tailPrefix}{Tail}]");
// #endif
//                     entity.ToTable($"{tableName}{tailPrefix}{Tail}");
//                 });
//             }
//             else
//             {
//                 BuildEntityTypeConfigurationCaches();
//                 //支持IEntityTypeConfiguration配置
//                 foreach (var entityTypeConfigurationType in _entityTypeConfigurationTypeCaches)
//                 {
//                     var shardingEntityType = entityTypeConfigurationType.Key;
//                     if (RemoveRemoveShardingEntity && shardingEntityType.IsShardingEntity())
//                         continue;
//                     var method = modelBuilder.GetType()
//                         .GetMethods()
//                         .FirstOrDefault(x => x.Name == nameof(ModelBuilder.ApplyConfiguration)
//                                              && x.GetParameters().Count() == 1
//                                              && x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
//                     method.MakeGenericMethod(shardingEntityType).Invoke(modelBuilder, new object[] {Activator.CreateInstance(entityTypeConfigurationType.Value)});
//                 }
//             }
//
//             ////字段注释,需要开启程序集XML文档
//
//             //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
//             //{
//             //    var comments = XmlHelper.GetPropertyCommentBySummary(entityType.ClrType) ?? new Dictionary<string, string>();
//             //    foreach (var property in entityType.GetProperties())
//             //    {
//             //        if (comments.ContainsKey(property.Name))
//             //        {
//             //            property.SetComment(comments[property.Name]);
//             //        }
//             //    }
//             //}
// //
// #if !EFCORE2
//             //字段注释,需要开启程序集XML文档
//             foreach (var entityType in modelBuilder.Model.GetEntityTypes())
//             {
//                 var comments = XmlHelper.GetProperyCommentBySummary(entityType.ClrType) ?? new Dictionary<string, string>();
//                 foreach (var property in entityType.GetProperties())
//                 {
//                     if (comments.ContainsKey(property.Name))
//                     {
//                         property.SetComment(comments[property.Name]);
//                     }
//                 }
//             }
// #endif
//         }
//
//         /// <summary>
//         /// 构建类型
//         /// </summary>
//         public void BuildEntityTypeConfigurationCaches()
//         {
//             if (!_entityTypeConfigurationTypeCaches.Any())
//             {
//                 lock (buildEntityTypeConfigurationLock)
//                 {
//                     if (!_entityTypeConfigurationTypeCaches.Any())
//                     {
//                         var typesToRegister = AssemblyHelper.CurrentDomain.GetAssemblies().SelectMany(o => o.GetTypes())
//                             .Where(type => !String.IsNullOrEmpty(type.Namespace))
//                             //获取类型namespce不是空的所有接口是范型的当前范型是IEntityTypeConfiguration<>的进行fluent api 映射
//                             .Where(type => !type.IsAbstract && type.GetInterfaces()
//                                 .Any(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
//                                            && it.GetGenericArguments().Any())
//                             ).ToDictionary(o => o.GetInterfaces().FirstOrDefault(it => it.IsInterface && it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
//                                                                                        && it.GetGenericArguments().Any())
//                                 ?.GetGenericArguments()[0], o => o);
//                         foreach (var type in typesToRegister)
//                         {
//                             _entityTypeConfigurationTypeCaches.TryAdd(type.Key, type.Value);
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }