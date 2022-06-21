using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class WriteDataModel : PageModel
{

    private readonly AbstaractShardingDbContext db;

    public int Count { get; set; }

    public WriteDataModel(AbstaractShardingDbContext db)
    {
        this.db = db;
    }

    public async Task OnGetAsync()
    {
        for (int i = 0; i < 30; i++)
        {
            //var dbFieldArr = new string[] { "11", "22" };
            //db.TestModels.Add(new TestModel { Content = i.ToString(), Description = DateTime.Now.Millisecond.ToString(), TestNewField = dbFieldArr.OrderBy(m => Guid.NewGuid()).First() });
            //db.Students.Add(new Student { Name = $"student_{i}" });
            db.Add(new Student { Name = $"student_{i}" });
            db.Add(new GuidShardingTable { Name = $"student_{i}" });
        }

        Count = await db.SaveChangesAsync();
    }
}
