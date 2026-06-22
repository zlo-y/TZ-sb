using Manager.BusinessLogic.DTOs;
using Manager.DataAccess.Entities;

namespace Manager.BusinessLogic.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetAllProjectsAsync(DateTime? startFrom, DateTime? startTo, int? priority, string sortBy, string order);
    Task CreateProjectAsync(ProjectServiceDto dto , string rootPath);
    Task<Project?> GetProjectByIdAsync(int id);
    Task UpdateProjectAsync(Project project);
    Task <bool> DeleteProjectAsync(int id);
    Task AddEmployeeProject(int projectId , int employeeId);
    Task RemoveEmployeeFromProjectAsync(int projectId, int employeeId);

}

