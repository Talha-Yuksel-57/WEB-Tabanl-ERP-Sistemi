namespace ERP.Core.DTOs.ProjectTask
{
    public class ProjectTaskDto
    {
        public int Id { get; set; }
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public int HoursWorked { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedDeveloperId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateProjectTaskDto
    {
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public int ProjectId { get; set; }
        public int? AssignedDeveloperId { get; set; }
        public string Priority { get; set; } = "Medium";
    }

    public class UpdateProjectTaskDto
    {
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public int HoursWorked { get; set; }
    }
}
