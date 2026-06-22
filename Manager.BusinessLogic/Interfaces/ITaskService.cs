using Manager.BusinessLogic.DTOs;
using Manager.DataAccess.Entities;
using TaskStatus = Manager.DataAccess.Enums.TaskStatus;

namespace Manager.BusinessLogic.Interfaces;

public interface ITaskService
{
    Task<List<ProjectTask>> GetProjectTasksAsync(int? projectId, TaskStatus? status, string sortBy, string order);
    Task CreateTaskAsync(TaskCreateDto task);
    Task AssignTaskExecutorAsync(int taskId, int? executorId);
    Task UpdateTaskStatusAsync(int taskId, TaskStatus status);

}