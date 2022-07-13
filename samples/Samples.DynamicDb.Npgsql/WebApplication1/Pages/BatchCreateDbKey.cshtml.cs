using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Data;
using WebApplication1.Data.Helpers;
using WebApplication1.Data.Models;
using WebApplication1.Data.Sharding;

namespace WebApplication1.Pages
{
    public class BatchCreateDbKeyModel : PageModel
    {

        private readonly AbstaractShardingDbContext db;
        private readonly IShardingRuntimeContext runtimeContext;

        [BindProperty]
        public BatchCreateModel CreateModel { get; set; }

        public BatchCreateDbKeyModel(AbstaractShardingDbContext db, IShardingRuntimeContext runtimeContext)
        {
            this.db = db;
            this.runtimeContext = runtimeContext;
        }

        public void OnGet()
        {
            CreateModel = new BatchCreateModel();
        }

        public IActionResult OnPost()
        {
            var keyList = new List<string>();
            for (int i = CreateModel.StarNum; i < CreateModel.EndNum; i++)
            {
                keyList.Add(i.ToString().PadLeft(CreateModel.Len, '0'));
            }

            // 读取并写入到配置
            var dblist = JsonFileHelper.Read<List<string>>(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName);
            dblist.AddRange(keyList);
            JsonFileHelper.Save(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName, dblist);

            foreach (var item in keyList)
            {
                db.TestModelKeys.Add(new TestModelKey { Key = item });
            }
            db.SaveChanges();

            foreach (var item in keyList)
            {
                // 动态新增数据源
                DynamicShardingHelper.DynamicAppendDataSourceOnly(runtimeContext, item, $"server=127.0.0.1;port=5432;uid=postgres;pwd=3#SanJing;database=shardingCoreDemo_{item};");
            }

            db.Database.Migrate();


            return RedirectToPage("DbKeyMan");
        }
    }

    public class BatchCreateModel
    {
        [Display(Name = "编号长度")]
        public int Len { get; set; } = 3;

        [Display(Name = "起始编号")]
        public int StarNum { get; set; } = 1;

        [Display(Name = "结束编号")]
        public int EndNum { get; set; } = 300;

    }
}
