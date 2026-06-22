using Manager.BusinessLogic.Interfaces;
using Manager.DataAccess;
using Manager.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Manager.BusinessLogic.Services;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Employee>> GetAllEmployeesAsync(string search)
    {
        var query = _context.Employees.AsQueryable();

        /// Case-insensitive search across name fields
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(e => 
                e.Name.ToLower().Contains(search) || 
                e.LastName.ToLower().Contains(search) ||
                e.MiddleName.ToLower().Contains(search));
        }

        return await query.ToListAsync();
    }

    public async Task CreateEmployeeAsync(Employee employee)
    {
        /// Add to tracker and persist changes
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateEmployeeAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        /// Check if exists before removing
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }
}