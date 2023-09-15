using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) 
        {
               
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1,DisplayOrder = 1,Name = "Action"},
                new Category { CategoryId = 2,DisplayOrder = 2,Name = "SciFi"},
                new Category { CategoryId = 3,DisplayOrder = 3,Name = "History"}
                );
        }
    }
}
