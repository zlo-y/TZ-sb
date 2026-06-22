using Xunit;
using Microsoft.EntityFrameworkCore;
using Manager.BusinessLogic.Services;
using Manager.DataAccess;
using Manager.DataAccess.Entities;
using Manager.BusinessLogic.DTOs;
using TaskStatus = Manager.DataAccess.Enums.TaskStatus;

public class ManagementSystemTests
{
    private AppDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    /// Checks the full lifecycle of project creation and management of employee-project relations
    [Fact]
    public async Task Project_FullCycle_RequirementCheck()
    {
        var db = GetContext();
        var service = new ProjectService(db);
        
        db.Employees.Add(new Employee { Id = 1, Name = "Manager" });
        await db.SaveChangesAsync();

        var dto = new ProjectServiceDto { 
            Name = "Project A", ManagerId = 1, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5) 
        };
        await service.CreateProjectAsync(dto, "./");
        
        /// Verify project was saved correctly
        var projects = await service.GetAllProjectsAsync(null, null, null, null, null);
        Assert.Single(projects);
        Assert.Equal("Project A", projects.First().Name);

        db.Employees.Add(new Employee { Id = 2, Name = "Executor" });
        await db.SaveChangesAsync();
        
        /// Check relation link between project and employee
        await service.AddEmployeeProject(1, 2);
        Assert.True(await db.ProjectEmployees.AnyAsync(pe => pe.ProjectId == 1 && pe.EmployeeId == 2));
    }

    /// Checks the full lifecycle of task creation, executor assignment, and status filtering
    [Fact]
    public async Task Tasks_FullCycle_RequirementCheck()
    {
        var db = GetContext();
        var service = new TaskService(db);
        
        db.Projects.Add(new Project { Id = 1, Name = "P1" });
        db.Employees.Add(new Employee { Id = 1, Name = "Author" });
        await db.SaveChangesAsync();

        /// Use DTO to create task and ensure it appears in the database
        var taskDto = new TaskCreateDto { 
            Name = "Task 1", 
            ProjectId = 1, 
            Comments = "Test comment", 
            Priority = 1 
        };
        
        await service.CreateTaskAsync(taskDto);
        
        /// Fetch saved task to verify ID and state
        var savedTask = await db.ProjectTasks.FirstAsync();
        
        /// Assign employee and verify status update transition
        await service.AssignTaskExecutorAsync(savedTask.Id, 1);
        
        await service.UpdateTaskStatusAsync(savedTask.Id, TaskStatus.Done);
        
        /// Ensure filtering by status returns the correct task
        var doneTasks = await service.GetProjectTasksAsync(1, TaskStatus.Done, null, null);
        Assert.Single(doneTasks);
    }
}