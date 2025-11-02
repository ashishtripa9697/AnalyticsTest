using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAnalyticsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context) { 
            _context = context;
        }


        // GET: /api/Analytics/salary-extremes
        [HttpGet("salary-extremes")]
        public async Task<ActionResult<List<SalaryExtremesDto>>> GetSalaryExtremes()
        {
            // current SalariesTble only
            var currentSalariesTble = _context.SalariesTble.AsNoTracking().Where(s => s.EffectiveTo == null);


            var q = from e in _context.EmployeesTble.AsNoTracking()
                    join s in currentSalariesTble on e.Id equals s.EmployeeId
                    join d in _context.DepartmentsTble.AsNoTracking() on e.DepartmentId equals d.DId
                    group new { e, s } by new { d.DId, d.DName } into g
                    select new SalaryExtremesDto
                    {
                        Department = g.Key.DName,
                        HighestSalary = g.Max(x => x.s.Amount),
                        LowestSalary = g.Min(x => x.s.Amount),
                        EmployeeCount = g.Select(x => x.e.Id).Distinct().Count()
                    };


            var result = await q.OrderBy(x => x.Department).ToListAsync();
            return Ok(result);
        }


        // GET: /api/Analytics/average-salary
        [HttpGet("average-salary")]
        public async Task<ActionResult<AverageSalaryDto>> GetAverageSalary()
        {

            var currentSalaries = _context.SalariesTble.AsNoTracking().Where(s => s.EffectiveTo == null);

            // var avg = await currentSalaries.Select(s => s.Amount).DefaultIfEmpty(0).AverageAsync();
            var avgnullable = await currentSalaries.Select(s => (decimal?)s.Amount).AverageAsync();

            var avg = Math.Round((avgnullable ?? 0m), 2);

            return Ok(new AverageSalaryDto { AverageSalary = avg });
        }


        // GET: /api/Analytics/top-department
        [HttpGet("top-department")]
        public async Task<ActionResult<TopDepartmentDto>> GetTopDepartment()
        {
            var currentSalariesTble = _context.SalariesTble.AsNoTracking().Where(s => s.EffectiveTo == null);


            var q = from e in _context.EmployeesTble.AsNoTracking()
                    join s in currentSalariesTble on e.Id equals s.EmployeeId
                    join d in _context.DepartmentsTble.AsNoTracking() on e.DepartmentId equals d.DId
                    group e by new { d.DId, d.DName } into g
                    select new TopDepartmentDto
                    {
                        Department = g.Key.DName,
                        EmployeeCount = g.Select(x => x.Id).Distinct().Count()
                    };



            var top = await q.OrderByDescending(x => x.EmployeeCount).ThenBy(x => x.Department).FirstOrDefaultAsync();
            if (top == null) return NotFound();
            return Ok(top);
        }
    }
}
