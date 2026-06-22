using Microsoft.AspNetCore.Mvc;
using Manager.BusinessLogic.Interfaces;
using Manager.DataAccess.Entities;
using TaskStatus = Manager.DataAccess.Enums.TaskStatus;
using Manager.BusinessLogic.DTOs;

namespace Manager.WebAPI.Controllers;

/// Controller for managing project tasks
/// Handles CRUD operations and status/assignment updates
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectTask>>> GetProjectTasks([FromQuery] int? projectId, [FromQuery] TaskStatus? status,
        [FromQuery] string? sortBy, [FromQuery] string? order)
    {
        /// Get filtered and sorted list of tasks
        var tasks = await _taskService.GetProjectTasksAsync(projectId, status, sortBy ?? "", order ?? "");
        return Ok(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
    {
        try 
        {
            /// Validate and create a new task
            await _taskService.CreateTaskAsync(dto);
            return StatusCode(201); 
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:int}/assign")]
    public async Task<IActionResult> AssignTask(int id, [FromBody] int? executorId)
    {
        try
        {
            /// Update the assigned employee for the task
            await _taskService.AssignTaskExecutorAsync(id, executorId);
            return NoContent();
        }
        catch(ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromQuery] TaskStatus status)
    {
        try
        {
            /// Change current task progress status
            await _taskService.UpdateTaskStatusAsync(id, status);
            return NoContent();
        }
        catch(ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }
}