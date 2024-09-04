using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Models;
using Task = TaskManagement.Domain.Models.Task;

namespace TaskManagement
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Task> Tasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Task>().HasOne(t => t.User)
                                       .WithMany(u => u.Tasks)
                                       .HasForeignKey(t => t.UserId);
        }
    }
}
