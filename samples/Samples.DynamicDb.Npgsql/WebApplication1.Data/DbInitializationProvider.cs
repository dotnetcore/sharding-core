using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1.Data
{

    /// <summary>
    /// 数据库初始化程序
    /// </summary>
    public class DbInitializationProvider
    {

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="services">IServiceProvider</param>
        public static void Initialize<T>(IServiceProvider services) where T : DbContext
        {
            var context = services.GetRequiredService<T>();
            if (context == null) throw new Exception("容器中未能找到 dbcontext 服务");

            // 确保上下文数据库存在。 如果存在，则不采取任何措施。 如果它不存在，那么将创建数据库及其所有架构。
            // 如果数据库存在，则不做任何努力以确保它与此上下文的模型兼容。
            // 请注意，此API不使用迁移来创建数据库。 此外，创建的数据库以后无法使用迁移进行更新。 如果您以关系数据库为目标并使用迁移，则可以使用DbContext.Database.Migrate（）方法来确保已创建数据库并应用了所有迁移。
            // context.Database.EnsureCreated();

            //判断是否有待迁移
            if (context.Database.GetPendingMigrations().Any())
            {
                //执行迁移
                context.Database.Migrate();
            }
        }

    }
}
