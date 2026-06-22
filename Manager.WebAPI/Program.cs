using Microsoft.EntityFrameworkCore;
using Manager.DataAccess;
using Manager.BusinessLogic.Interfaces;
using Manager.BusinessLogic.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка стандартных сервисов API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (чтобы фронтенд мог достучаться)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

// Настройка подключения к SQLite базы данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация сервисов бизнес-логики в DI-контейнер
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();   
// app.UseHttpsRedirection();

// CORS Middleware 
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();