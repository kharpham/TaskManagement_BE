using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagementApp.Data;
using TaskManagementApp.Dtos;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase 
    {
        private readonly ApplicationDbContext _context;
        public TaskController(ApplicationDbContext context)
        {
            _context = context; 
        }
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTask task)
        {
            System.Diagnostics.Debug.WriteLine("Creating task...");
            if (task == null)
            {
                return BadRequest("Invalid task data");
            }
            var userId = HttpContext.Items["UserId"] as string;
            if (userId == null)
            {
                System.Diagnostics.Debug.WriteLine("User ID is not found...");
                return Unauthorized("You are not authorized");
            }
            var assignedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == task.AssignedTo);
            if (assignedUser == null)
            {
                return BadRequest("Assigned user does not exist");
            }
            var newTask = new TaskManagementApp.Models.Task
            {
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                AssignedTo = task.AssignedTo,
                IsComplete = false,
                User = assignedUser,
            };
            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Ok(newTask);
        } 
    }
}
