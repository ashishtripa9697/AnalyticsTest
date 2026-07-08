using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using EmployeeAnalyticsAPI.Interface;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAnalyticsAPI.Services
{
    public class SalaryService : ISalary
    {
        private readonly AppDbContext _context;
        public SalaryService(AppDbContext context) 
        {
            _context = context;
        }

        public async Task<List<SalaryExtremesDto>> GetSalaryExtremes()
        {
            var currentsalaryTable = _context.SalariesTble.AsNoTracking().Where(s=>s.EffectiveTo == null);
            var q = from e in _context.EmployeesTble.AsNoTracking()
                    join s in currentsalaryTable on e.Id equals s.EmployeeId
                    join d in _context.DepartmentsTble.AsNoTracking() on e.DepartmentId equals d.DId
                    group new { e, s } by new { d.DId, d.DName } into g
                    select new SalaryExtremesDto
                    {
                        Department = g.Key.DName,
                        HighestSalary = g.Max(x => x.s.Amount),
                        LowestSalary = g.Min(x => x.s.Amount),
                        EmployeeCount = g.Select(x => x.e.Id).Distinct().Count()
                    };
            var result= await q.OrderBy(x => x.Department).ToListAsync();
            return result;
        }

        public async Task<List<AverageSalaryDto>> GetAverageSalary()
        {
            var currentSalaries = _context.SalariesTble.AsNoTracking().Where(s => s.EffectiveTo == null);
            var avgnullable = await currentSalaries.Select(s => (decimal?)s.Amount).AverageAsync();
            var avg = Math.Round((avgnullable ?? 0m), 2);
            //return new AverageSalaryDto { AverageSalary = avg };
            return new List<AverageSalaryDto>
            {
                new AverageSalaryDto { AverageSalary = avg }
            };
        }

        public async Task<TopDepartmentDto> GetTopDepartment()
        {
            var currentsalaryTable = _context.SalariesTble.AsNoTracking().Where(s => s.EffectiveTo == null);
            var q = from e in _context.EmployeesTble.AsNoTracking()
                    join s in currentsalaryTable on e.Id equals s.EmployeeId
                    join d in _context.DepartmentsTble.AsNoTracking() on e.DepartmentId equals d.DId
                    group e by new { d.DId, d.DName } into g
                    select new TopDepartmentDto
                    {
                        Department = g.Key.DName,
                        EmployeeCount = g.Select(x => x.Id).Distinct().Count()
                    };
            var result = await q.OrderByDescending(x => x.EmployeeCount).FirstOrDefaultAsync();
            return result;
        }
    }
}