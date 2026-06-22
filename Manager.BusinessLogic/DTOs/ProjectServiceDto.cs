using Microsoft.AspNetCore.Http;

namespace Manager.BusinessLogic.DTOs;

/// DTO for handling project creation requests including files
public class ProjectServiceDto
{
    public string Name { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Executor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Priority { get; set; }

    public int ManagerId { get; set; }
    public List<int> ExecutorId { get; set; } = new();

    /// Files attached during project creation
    public List<IFormFile> UploadedFiles { get; set; } = new();
}