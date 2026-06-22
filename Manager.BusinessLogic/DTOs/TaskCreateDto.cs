namespace Manager.BusinessLogic.DTOs;

/// DTO for creating a new task and linking it to a project
public class TaskCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Comments { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int ProjectId { get; set; }
    public int? ExecutorId { get; set; }
}