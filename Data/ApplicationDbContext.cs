﻿using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;

namespace TaskManagementApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base (options)
        {   
        }
        public DbSet<TaskManagementApp.Models.Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
