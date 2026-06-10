using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAnalyticsAPI.DataL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Department> DepartmentsTble => Set<Department>();

        public virtual DbSet<Employee> EmployeesTble => Set<Employee>();

        public virtual DbSet<Salary> SalariesTble => Set<Salary>();
        public virtual DbSet<EmployeeChunk> EmployeeChunksTble => Set<EmployeeChunk>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Department>(b =>
            {
                b.HasKey(d => d.DId);
                b.Property(d => d.DName).HasMaxLength(100).IsRequired();
                b.HasIndex(d => d.DName).IsUnique();
            });

            modelBuilder.Entity<Employee>(b =>
            {
                b.HasKey(e => e.Id);
                b.Property(e => e.FirstName).HasMaxLength(100);
                b.Property(e => e.LastName).HasMaxLength(100);
                b.HasOne(e => e.Department).WithMany(d => d.EmployeesTble).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
                b.HasIndex(e => e.DepartmentId);
            });

            modelBuilder.Entity<Salary>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Amount).HasColumnType("decimal(18,2)");
                b.HasOne(s => s.Employee).WithMany(e => e.SalariesTble).HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(s => s.EmployeeId);
            });
            // DataL/AppDbContext.cs — inside EmployeeChunk config
            modelBuilder.Entity<EmployeeChunk>(b =>
            {
                b.ToTable("EmployeeChunks");  

                b.HasKey(ec => ec.Id);

                b.Property(ec => ec.Content)
                 .IsRequired()
                 .HasColumnType("nvarchar(max)");

                b.Property(ec => ec.Embedding)
                 .IsRequired()
                 .HasColumnType("nvarchar(max)");

                b.Property(ec => ec.Department)
                 .HasMaxLength(100);

                b.Property(ec => ec.CreatedAt)
                 .HasColumnType("datetime2")
                 .HasDefaultValueSql("GETUTCDATE()");

                b.HasOne(ec => ec.Employee)
                 .WithMany(e => e.EmployeeChunksTble)
                 .HasForeignKey(ec => ec.EmployeeId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(ec => ec.EmployeeId);
            });
        }
    }
}
