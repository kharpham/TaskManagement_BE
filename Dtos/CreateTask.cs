namespace TaskManagementApp.Dtos
{
    public class CreateTask
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public DateTime DueDate { get; set; }
        public required string AssignedTo { get; set; }
    }
}
