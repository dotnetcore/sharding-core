using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Data.Helpers;
using WebApplication1.Data.Models;
using WebApplication1.Data.Sharding;

namespace WebApplication1.Pages
{
    public class DbKeyManModel : PageModel
    {

        private readonly AbstaractShardingDbContext db;
        public IEnumerable<TestModelKey> DbKeyList { get; set; }

        public DbKeyManModel(AbstaractShardingDbContext db)
        {
            this.db = db;
        }

        public void OnGet()
        {
            DbKeyList = db.TestModelKeys.AsNoTracking().OrderBy(m => m.CreationDate).ToList();

            // Ð´Èëµ½ÅäÖÃ
            JsonFileHelper.Save(AppContext.BaseDirectory, TestModelVirtualDataSourceRoute.ConfigFileName, DbKeyList.Select(m => m.Key).ToList());
        }
    }
}
