using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.Interface;
using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAnalyticsAPI.Services
{
    public class EmployeeServices(AppDbContext context) : IEmployeeService
    {
        private readonly AppDbContext _context = context;

        public async Task <List<EmployeeDto>> GetAllEmployeeAsync(int pagenumber = 1, int pagesize = 20,CancellationToken ct=default)
        {
            if(pagenumber <1)
            {
                pagenumber = 1;
            }
            if(pagesize <1)
            {
                pagesize = 20;
            }
            if(pagesize > 100)
            {
                pagesize = 100;
            }
            var employees =  _context.EmployeesTble
                .Include(d => d.Department)
                .Include(s => s.SalariesTble)
                .Include(ed => ed.EmployeeDetails).AsSplitQuery();
                //.ToListAsync();
            var totalRecords =await employees.CountAsync(ct);
            var employee = employees.OrderBy(e => e.Id).Skip((pagenumber - 1) * pagesize).Take(pagesize).ToListAsync(ct);
            var totalpages= (int)Math.Ceiling((double)totalRecords / pagesize);
            //var employeeDtos = employees.Select(MapToDto).ToList();
            
            
            var employeeDtos=employees.Select(MapToDto).ToList();

           return employeeDtos;

        }

        public async Task<EmployeeDto> GetEmployeeByIdAsync(int employeeId)
        {
            var employee = await _context.EmployeesTble
                .Include(d => d.Department)
                .Include(s => s.SalariesTble)
                .Include(ed => ed.EmployeeDetails)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null)
            {
              return null;
            }

            var employeeDto = MapToDto(employee);

           return employeeDto;
        }

        private static EmployeeDto MapToDto(Employee employee)
        {
            var detail = employee.EmployeeDetails.FirstOrDefault();
            var currentSalary = employee.SalariesTble
                .OrderByDescending(s => s.EffectiveFrom)
                .FirstOrDefault();

            return new EmployeeDto
            {
                EmployeeId = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Salary = currentSalary?.Amount ?? 0,
                ContactNumber = null,
                DepartmentName = employee.Department?.DName ?? "—",
                HireDate = employee.HireDate,
                AadharNumber = detail?.AadharNumber,
                PanNumber = detail?.PanNumber,
            };
        }
    }
}