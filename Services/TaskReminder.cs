using TaskManagementApp.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using Task = System.Threading.Tasks.Task;

namespace TaskManagementApp.Services
{
    public class TaskReminder : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public TaskReminder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckForUpcomingDeadlines();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Check every hour
            }
        }

        private async Task CheckForUpcomingDeadlines()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var upcomingTasks = await context.Tasks
                    .Where(t => !t.IsComplete && t.DueDate <= DateTime.Now.AddHours(24) && !t.ReminderSent) // Tasks due in the next 24 hours and reminder not sent
                    .ToListAsync();

                foreach (var task in upcomingTasks)
                {
                    var user = await context.Users.FindAsync(task.UserId);
                    if (user != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Checking user...");
                        System.Diagnostics.Debug.WriteLine($"User ID: {user.Id}");
                        System.Diagnostics.Debug.WriteLine($"User Email: {user.Email}");

                        if (!string.IsNullOrEmpty(user.Email))
                        {
                            try
                            {
                                await emailService.SendEmailAsync(user.Email, "Task Reminder", $"Your task '{task.Title}' is due soon.");
                                task.ReminderSent = true; // Set ReminderSent to true
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"Failed to send email to {user.Email}: {ex.Message}");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("User email is null or empty.");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("User is null...");
                    }
                }

                await context.SaveChangesAsync(); // Save changes to the database
            }
        }
    }
}
