using Manager.BusinessLogic.Interfaces;
using Manager.DataAccess;
using Microsoft.EntityFrameworkCore;
using Manager.DataAccess.Entities;
using TaskStatus = Manager.DataAccess.Enums.TaskStatus;
using Manager.BusinessLogic.DTOs;

namespace Manager.BusinessLogic.Services;

public class TaskService : ITaskService{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectTask>> GetProjectTasksAsync(int? projectId, TaskStatus? status, string sortBy, string order)
    {
        /// Load tasks with navigation properties for UI display
        var tasks = _context.ProjectTasks
            .Include(p => p.Author)
            .Include(p => p.Executor)
            .AsQueryable();

        if(projectId.HasValue) tasks = tasks.Where(t => t.ProjectId == projectId.Value);
        if(status.HasValue) tasks = tasks.Where(t => t.Status == status.Value);

        /// Apply dynamic sorting
        bool ascending = order?.ToLower() == "desc";
        tasks = sortBy?.ToLower() switch
        {
            "name" => ascending ? tasks.OrderByDescending(t => t.Name) : tasks.OrderBy(t => t.Name),
            "status" => ascending ? tasks.OrderByDescending(t => t.Status) : tasks.OrderBy(t => t.Status),
            _ => tasks.OrderBy(t => t.Id)
        };
        return await tasks.ToListAsync();
    }

    public async Task CreateTaskAsync(TaskCreateDto dto)
    {
        var task = new ProjectTask
        {
            Name = dto.Name,
            Comments = dto.Comments,
            Priority = dto.Priority,
            ProjectId = dto.ProjectId,
            ExecutorId = dto.ExecutorId,
            Status = Manager.DataAccess.Enums.TaskStatus.ToDo, 
            AuthorId = 1 
        };

        /// Verify project exists before attaching task
        var projectExists = await _context.Projects.AnyAsync(p => p.Id == task.ProjectId);
        if (!projectExists) throw new Exception("Указанный проект не существует!");

        _context.ProjectTasks.Add(task);
        await _context.SaveChangesAsync();
    }

    public async Task AssignTaskExecutorAsync(int taskId, int? executorId)
    {
        /// Find task and update assigned executor
        var task = await _context.ProjectTasks.FindAsync(taskId);
        if (task == null){
            throw new ArgumentException($"Task with ID {taskId} not found.");}

        task.ExecutorId = executorId;
        await _context.SaveChangesAsync();
    }

    public async Task UpdateTaskStatusAsync(int taskId, TaskStatus status)
    {
        /// Find task and update current status
        var task = await _context.ProjectTasks.FindAsync(taskId);
        if (task == null)
        {
            throw new ArgumentException($"Task with ID {taskId} not found.");
        }
        task.Status = status;
        await _context.SaveChangesAsync();
    }
}