using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShardingCore.Helpers;
using System;
using System.Collections.Generic;
using WebApplication1.Data;
using WebApplication1.Data.Helpers;
using WebApplication1.Data.Models;
using WebApplication1.Data.Sharding;

namespace WebApplication1.Pages
{
    public class CreateDbKeyModel : PageModel
    {

        private readonly AbstaractShardingDbContext db;

        [BindProperty]
        public TestModelKey NewModel { get; set; }

        public CreateDbKeyModel(AbstaractShardingDbContext db)
        {
            this.db = db;
        }

        public void OnGet()
        {
            NewModel = new TestModelKey();
        }

        public IActionResult OnPost()
        {
            NewModel.Id = Guid.NewGuid();
            NewModel.CreationDate = DateTime.Now;
            db.TestModelKeys.Add(NewModel);
            db.SaveChanges();

            // 读取并写入到配置
            var dblist = JsonFileHelper.Read<List<string>>(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName);
            dblist.Add(NewModel.Key);
            dblist.Sort();
            JsonFileHelper.Save(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName, dblist);

            // 动态新增数据源
            DynamicShardingHelper.DynamicAppendDataSource<AbstaractShardingDbContext>("c1", NewModel.Key, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{NewModel.Key};");

            return RedirectToPage("DbKeyMan");
        }
    }
}
