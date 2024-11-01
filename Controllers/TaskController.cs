using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            if (task == null)
            {
                return BadRequest("Invalid task data");
            }

            var userId = HttpContext.Items["UserId"] as string;
            if (userId == null)
            {
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

        [HttpGet]
        public async Task<IActionResult> GetTasks()
        {
            var tasks = await _context.Tasks.ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            var userId = HttpContext.Items["UserId"] as string;
            if (userId == null)
            {
                return Unauthorized("You are not authorized");
            }

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound("Task not found");
            }

            return Ok(task);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = HttpContext.Items["UserId"] as string;
            if (userId == null)
            {
                return Unauthorized("You are not authorized");
            }

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound("Task not found");
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] CreateTask updatedTask)
        {
            if (updatedTask == null)
            {
                return BadRequest("Invalid task data");
            }

            var userId = HttpContext.Items["UserId"] as string;
            if (userId == null)
            {
                return Unauthorized("You are not authorized");
            }

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound("Task not found");
            }

            var assignedUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == updatedTask.AssignedTo);
            if (assignedUser == null)
            {
                return BadRequest("Assigned user does not exist");
            }

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.AssignedTo = updatedTask.AssignedTo;
            task.User = assignedUser;

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatus taskStatus)
        {
            if (taskStatus == null)
            {
                return BadRequest("Invalid status data");
            }

            var userId = HttpContext.Items["UserId"] as string;
            if (userId == null)
            {
                return Unauthorized("You are not authorized");
            }

            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null)
            {
                return NotFound("Task not found");
            }

            task.IsComplete = taskStatus.IsComplete;
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();

            return Ok(task);
        }
    }
}
