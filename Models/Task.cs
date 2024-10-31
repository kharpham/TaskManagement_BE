namespace TaskManagementApp.Models
{
    public class Task
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public bool IsComplete { get; set; }
        public DateTime DueDate { get; set; }
        public required string AssignedTo { get; set; }

        // Foreign Key

        //public required string UserId { get; set; }

        // Navigation Property
        public required User User { get; set; }
    }
}
