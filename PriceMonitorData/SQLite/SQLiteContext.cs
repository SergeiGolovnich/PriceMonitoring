using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PriceMonitorData.Models;
using PriceMonitorData.SQLite.Models;

namespace PriceMonitorData.SQLite
{
    class SQLiteContext : DbContext
    {
        public DbSet<ItemPOCO> Items { get; set; }
        public DbSet<Price> Prices { get; set; }
        public SQLiteContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=PriceMonitoringSQLiteDB.db");
        }
    }
}
