using CatalogMicroservice.Model;
using Microsoft.EntityFrameworkCore;

namespace CatalogMicroservice.Data
{
   
    public class DbContextClass: DbContext
    {
        protected readonly IConfiguration Configuration;
        public DbContextClass(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
        }
        public DbSet<Product> Products { get; set; }
        
    }
    
}
