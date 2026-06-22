using Manager.BusinessLogic.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Manager.DataAccess.Entities;
using Manager.BusinessLogic.DTOs;

namespace Manager.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProjectsController(IProjectService projectService, IWebHostEnvironment webHostEnvironment)
    {
        _projectService = projectService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects([FromQuery] DateTime? startFrom,
        [FromQuery] DateTime? startTo,
        [FromQuery] int? priority,
        [FromQuery] string? sortBy,
        [FromQuery] string? order)
    {
        /// Fetch filtered and sorted projects from service
        var projects = await _projectService.GetAllProjectsAsync(startFrom, startTo, priority, sortBy ?? "", order ?? "");
        return Ok(projects);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Project>> GetById(int id)
    {
        /// Return 404 if project doesn't exist
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound("Project not found");
        }
        return Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] ProjectServiceDto dto)
    {
        try
        {
            /// Pass web root path for potential file saving logic
            string rootPath = _webHostEnvironment.ContentRootPath;
            await _projectService.CreateProjectAsync(dto, rootPath);
            return StatusCode(201, "Project created successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Project project)
    {
        /// Ensure ID consistency between URL and request body
        if (id != project.Id)
        {
            return BadRequest("Project ID mismatch");
        }

        await _projectService.UpdateProjectAsync(project);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        /// Verify existence before returning success status
        var isDeleted = await _projectService.DeleteProjectAsync(id);
        if (!isDeleted)
        {
            return NotFound("Project not found");
        }
        return NoContent();
    }

    [HttpPost("{projectId}/employees/{employeeId}")]
    public async Task<IActionResult> AddEmployee(int projectId, int employeeId)
    {
        /// Link employee to project
        await _projectService.AddEmployeeProject(projectId, employeeId);
        return Ok();
    }

    [HttpDelete("{projectId}/employees/{employeeId}")]
    public async Task<IActionResult> RemoveEmployee(int projectId, int employeeId)
    {
        /// Unlink employee from project
        await _projectService.RemoveEmployeeFromProjectAsync(projectId, employeeId);
        return NoContent();
    }
}