using Microsoft.EntityFrameworkCore;
using RelateApp.API.Models;

namespace RelateApp.API.Data
{
    public class DbDataContext : DbContext
    {
        public DbDataContext(DbContextOptions<DbDataContext> options) : base (options)
        {
            
        }
        public DbSet<Value> Values { get; set; }
    }
}