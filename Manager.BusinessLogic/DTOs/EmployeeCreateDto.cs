namespace Manager.BusinessLogic.DTOs;

/// DTO for capturing new employee data from request
public class EmployeeCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}