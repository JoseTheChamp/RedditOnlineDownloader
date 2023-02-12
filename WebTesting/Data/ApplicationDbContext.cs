using Microsoft.EntityFrameworkCore;
using WebTesting.Models;

namespace WebTesting.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Category> Category { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
