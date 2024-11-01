using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Data;
using TaskManagementApp.Dtos;
using TaskManagementApp.Models;
using TaskManagementApp.Services;

namespace TaskManagementApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService; // Inject email service

        public TaskController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
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

            var newTask = new Models.Task
            {
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                AssignedTo = task.AssignedTo,
                IsComplete = false,
                User = assignedUser,
                UserId = assignedUser.Id,
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            // Send email notification to the assigned user
            await _emailService.SendEmailAsync(assignedUser.Email, "Task Assignment", $"You have been assigned to the task '{newTask.Title}'.");

            return Ok(newTask);
        }

        [HttpGet]
        public async Task<IActionResult> GetTasks([FromQuery] bool? isComplete)
        {
            var query = _context.Tasks.AsQueryable();

            if (isComplete.HasValue)
            {
                query = query.Where(t => t.IsComplete == isComplete.Value);
            }

            var tasks = await query.OrderByDescending(t => t.Id).ToListAsync();
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

            task.User = assignedUser;
            // Send email notification to the assigned user if the assignment has changed
            if (task.AssignedTo != updatedTask.AssignedTo)
            {
                task.AssignedTo = updatedTask.AssignedTo;
                task.ReminderSent = false;
                await _emailService.SendEmailAsync(assignedUser.Email, "Task Assignment", $"You have been assigned to the task '{task.Title}'.");
            }
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
