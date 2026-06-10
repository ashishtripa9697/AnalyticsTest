using EmployeeAnalyticsAPI.DataL;
using EmployeeAnalyticsAPI.Interface;
using EmployeeAnalyticsAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ISalary, SalaryService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
//Context Register
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not set");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<IEmployeeRagService, EmployeeRagService>();

builder.Services.AddTransient<DataSeeder>();
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


