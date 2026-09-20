using Bogus;
using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;


namespace EmployeeAnalyticsAPI.DataL
{
    public class DataSeeder
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public DataSeeder(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task SeedIfNeededAsync()
        {
            if (await _db.DepartmentsTble.AnyAsync())
                return; // already seeded

            var deptNames = new[] { "Engineering", "Sales", "HR", "Finance", "Marketing" };
            var DepartmentsTble = deptNames.Select(n => new Department { DName = n }).ToList();
            await _db.DepartmentsTble.AddRangeAsync(DepartmentsTble);
            await _db.SaveChangesAsync();

            var EmployeesTbleCount = _config.GetValue<int>("SeedOptions:EmployeesTbleCount", 7000);

            var faker = new Faker("en");
            var empId = 1;
            var EmployeesTble = new List<Employee>(EmployeesTbleCount);
            var SalariesTble = new List<Salary>(EmployeesTbleCount);

            var rnd = new Random();

            for (int i = 0; i < EmployeesTbleCount; i++)
            {
                var dept = DepartmentsTble[rnd.Next(DepartmentsTble.Count)];
                var hireDate = faker.Date.Past(10);

                var emp = new Employee
                {
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    HireDate = hireDate,
                    DepartmentId = dept.DId
                };
                EmployeesTble.Add(emp);
            }

            // Bulk insert EmployeesTble


            await _db.EmployeesTble.AddRangeAsync(EmployeesTble);
            await _db.SaveChangesAsync();

            // Now add SalariesTble (one current salary per employee, some variance)


            var allEmployeesTble = await _db.EmployeesTble.AsNoTracking().ToListAsync();
            foreach (var e in allEmployeesTble)
            {
                // set base salary by department to create variance


                decimal baseSalary = e.DepartmentId switch
                {
                    1 => 90000m, // Engineering
                    2 => 60000m, // Sales
                    3 => 45000m, // HR
                    4 => 70000m, // Finance
                    5 => 50000m, // Marketing
                    _ => 50000m
                };

                // random +/- up to 40%
                var factor = 1m + ((decimal)(new Random(e.Id).NextDouble() - 0.5) * 0.8m);
                var amount = Math.Round(baseSalary * factor, 2);

                SalariesTble.Add(new Salary
                {
                    EmployeeId = e.Id,
                    Amount = amount,
                    EffectiveFrom = e.HireDate,
                    EffectiveTo = null
                });
            }

            await _db.SalariesTble.AddRangeAsync(SalariesTble);
            await _db.SaveChangesAsync();
        }
    }
}

