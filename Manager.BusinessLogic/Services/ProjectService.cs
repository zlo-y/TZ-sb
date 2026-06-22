using Manager.BusinessLogic.Interfaces;
using Manager.DataAccess.Entities;
using Manager.DataAccess;
using Microsoft.EntityFrameworkCore;
using Manager.BusinessLogic.DTOs;

namespace Manager.BusinessLogic.Services;

public class ProjectService: IProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllProjectsAsync(DateTime? startFrom, DateTime? startTo, int? priority, string sortBy, string order)
    {
        /// Load projects with manager details for display
        var projects = _context.Projects.Include(p => p.Manager).AsQueryable();

        if(startFrom.HasValue) projects = projects.Where(p => p.StartDate >= startFrom.Value);
        if(startTo.HasValue) projects = projects.Where(p => p.StartDate <= startTo.Value);
        if(priority.HasValue) projects = projects.Where(p => p.Priority == priority.Value);

        /// Dynamic sorting logic based on query parameters
        bool ascending = order?.ToLower() == "desc";
        projects = sortBy?.ToLower() switch
        {
            "name" => ascending ? projects.OrderByDescending(p => p.Name) : projects.OrderBy(p => p.Name),
            "priority" => ascending ? projects.OrderByDescending(p => p.Priority) : projects.OrderBy(p => p.Priority),
            "startdate" => ascending ? projects.OrderByDescending(p => p.StartDate) : projects.OrderBy(p => p.StartDate),
            _ => projects.OrderBy(p => p.Id)
        };

        return await projects.ToListAsync();
    }


    public async Task CreateProjectAsync(ProjectServiceDto dto, string rootPath)
    {
        /// Validate manager exists before creating project
        var managerExists = await _context.Employees.AnyAsync(e => e.Id == dto.ManagerId);
        
        if (!managerExists)
        {
            throw new ArgumentException($"Ошибка создания проекта: Выбранный руководитель (ID: {dto.ManagerId}) не найден в системе.");
        }
        if(dto.StartDate > dto.EndDate)
            throw new ArgumentException("Start date must be before end date.");


        var project = new Project
        {
            Name = dto.Name,
            Customer = dto.Customer,
            Executor = dto.Executor,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Priority = dto.Priority,
            ManagerId = dto.ManagerId
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        /// Link selected employees to the new project
        if (dto.ExecutorId != null && dto.ExecutorId.Any())
        {
            foreach (var empId in dto.ExecutorId)
            {
                _context.ProjectEmployees.Add(new ProjectEmployee
                {
                    ProjectId = project.Id,
                    EmployeeId = empId
                });
            }
        }

        /// Handle file uploads and save paths to database
        if (dto.UploadedFiles != null && dto.UploadedFiles.Count > 0)
        {
            var uploadsFolder = Path.Combine(rootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            foreach (var file in dto.UploadedFiles)
            {
                if (file.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    _context.ProjectDocuments.Add(new ProjectDocument
                    {
                        ProjectId = project.Id,
                        FileName = file.FileName,
                        FilePath = Path.Combine("uploads", uniqueFileName)
                    });
                }
            }
        }
        await _context.SaveChangesAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        /// Load project with all related entities
        return await _context.Projects
            .Include(p => p.Manager)
            .Include(p => p.ProjectEmployees).ThenInclude(pe => pe.Employee)
            .Include(p => p.ProjectTasks)
            .Include(p => p.ProjectDocuments)
            .FirstOrDefaultAsync(p => p.Id == id);
    } 

    public async Task UpdateProjectAsync(Project project)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AddEmployeeProject(int projectId, int employeeId)
    {
        /// Add employee to project only if relation doesn't exist
        var exists = await _context.ProjectEmployees.AnyAsync(pe => pe.ProjectId == projectId && pe.EmployeeId == employeeId);
        if (!exists)
        {
            _context.ProjectEmployees.Add(new ProjectEmployee { ProjectId = projectId, EmployeeId = employeeId });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveEmployeeFromProjectAsync(int projectId, int employeeId)
    {
        /// Find and remove existing employee-project link
        var link = await _context.ProjectEmployees.FirstOrDefaultAsync(pe => pe.ProjectId == projectId && pe.EmployeeId == employeeId);
        if (link != null)
        {
            _context.ProjectEmployees.Remove(link);
            await _context.SaveChangesAsync();
        }
    }
}