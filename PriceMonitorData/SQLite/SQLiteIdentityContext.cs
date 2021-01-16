using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriceMonitorData.Models;
using PriceMonitorData.SQLite.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PriceMonitorData.SQLite
{
    public class SQLiteIdentityContext : IdentityDbContext<IdentityUser>
    {
        public SQLiteIdentityContext(DbContextOptions<SQLiteIdentityContext> options) : base(options) 
        { 
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=PriceMonitoringSQLiteIdentityDB.db");
        }
    }
}
