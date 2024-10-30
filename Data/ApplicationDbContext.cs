using Microsoft.AspNetCore.Identity.EntityFrameworkCore;    
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<TaskManagementApp.Models.Task> Tasks { get; set; }
    }
}
