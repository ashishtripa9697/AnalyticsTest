using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.Interface;
using EmployeeAnalyticsAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ISalary, SalaryService>();//Register with every time created epr every request
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Context Register
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not set");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

builder.Services.AddTransient<DataSeeder>();//Register with every time created epr every request
var app = builder.Build();

// Configure the HTTP request pipeline.
using (var scop = app.Services.CreateScope())
{
    var db=scop.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    var seeder = scop.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedIfNeededAsync();
}

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }


app.UseHttpsRedirection();

app.MapControllers();
app.Run();


