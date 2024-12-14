using Microsoft.EntityFrameworkCore;
using UserMicroservice.Model;

namespace UserMicroservice.Data
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
        public DbSet<User> Users { get; set; }
        
    }
}
