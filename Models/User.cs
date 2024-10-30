using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Collections.Generic;   
namespace TaskManagementApp.Models
{
    public class User : IdentityUser
    {
        public required ICollection<Task> Tasks { get; set; } 
    }
}
