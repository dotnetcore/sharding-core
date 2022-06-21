using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using WebApplication1.Data;
using WebApplication1.Data.Models;

namespace WebApplication1.Pages
{
    public class ListDataModel : PageModel
    {

        private readonly AbstaractShardingDbContext db;
        public IEnumerable<Student>? StudentList { get; set; }
        public IEnumerable<TestModel>? TestModelList { get; set; }

        public ListDataModel(AbstaractShardingDbContext db)
        {
            this.db = db;
        }

        public void OnGet()
        {
            StudentList = db.Students.AsNoTracking().OrderBy(m => m.Id).Take(10).ToList();
            //TestModelList = db.TestModels.AsNoTracking().OrderBy(m => m.CreationTime).Take(10).ToList();
        }

    }
}
