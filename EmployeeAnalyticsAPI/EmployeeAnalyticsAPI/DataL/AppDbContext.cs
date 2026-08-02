using EmployeeAnalyticsAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAnalyticsAPI.DataL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
        public virtual DbSet<Department> DepartmentsTble => Set<Department>();
        public virtual DbSet<Employee> EmployeesTble => Set<Employee>();
        public virtual  DbSet<EmployeeDetail> EmployeeDetails=> Set<EmployeeDetail>();
        public virtual DbSet<Salary> SalariesTble => Set<Salary>();
        public virtual DbSet<EmployeeChunk> EmployeeChunksTble => Set<EmployeeChunk>();
        public virtual DbSet<DocumentChunk> DocumentChunksTble => Set<DocumentChunk>();
        public virtual DbSet<ErrorLog> ErrorsTble => Set<ErrorLog>();


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
            modelBuilder.Entity<EmployeeDetail>(b =>
            {
                b.ToTable("EmployeeDetailsTble");
                b.HasKey(e => e.Id);

                b.Property(e => e.EmployeeId)
                 .IsRequired(false);

                b.Property(e => e.BankName)
                 .HasMaxLength(100)
                 .HasColumnType("varchar(100)");

                b.Property(e => e.AccountNumber)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");

                b.Property(e => e.UANNumber)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");

                b.Property(e => e.PFNumber)
                 .HasMaxLength(50)
                 .HasColumnType("varchar(50)");

                b.Property(e => e.PanNumber)
                 .HasMaxLength(20)
                 .HasColumnType("varchar(20)");

                b.Property(e => e.AadharNumber)
                 .HasMaxLength(20)
                 .HasColumnType("varchar(20)");

                b.Property(e => e.Address)
                 .HasMaxLength(500)
                 .HasColumnType("varchar(500)");

                b.Property(e => e.City)
                 .HasMaxLength(100)
                 .HasColumnType("varchar(100)");

                b.Property(e => e.State)
                 .HasMaxLength(100)
                 .HasColumnType("varchar(100)");

                b.Property(e => e.CreatedAt)
                 .HasColumnType("datetime2")
                 .HasDefaultValueSql("GETUTCDATE()");

                b.HasOne(s => s.Employee)
                 .WithMany(e => e.EmployeeDetails)
                 .HasForeignKey(s => s.EmployeeId)
                 .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(e => e.EmployeeId);
            });
            modelBuilder.Entity<Salary>(b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Amount).HasColumnType("decimal(18,2)");
                b.HasOne(s => s.Employee).WithMany(e => e.SalariesTble).HasForeignKey(s => s.EmployeeId).OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(s => s.EmployeeId);
            });
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
            modelBuilder.Entity<DocumentChunk>(b =>
            {
                b.ToTable("DocumentChunks");
                b.HasKey(d => d.Id);

                b.Property(d => d.FileName)
                 .IsRequired()
                 .HasMaxLength(255);

                b.Property(d => d.Content)
                 .IsRequired()
                 .HasColumnType("nvarchar(max)");

                b.Property(d => d.Embedding)
                 .IsRequired()
                 .HasColumnType("nvarchar(max)");

                b.Property(d => d.PageNumber)
                 .IsRequired();

                b.Property(d => d.CreatedAt)
                 .HasColumnType("datetime2")
                 .HasDefaultValueSql("GETUTCDATE()");

                b.HasIndex(d => d.FileName);
            });
            modelBuilder.Entity<ErrorLog>(b =>
            {
                b.ToTable("ErrorLogs");
                b.HasKey(e=>e.Id);
                b.Property(e => e.TraceId)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");
                b.Property(e => e.RequestPath)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

                b.Property(e => e.RequestMethod)
                 .HasMaxLength(10)
                 .HasColumnType("varchar(10)");

                b.Property(e => e.ExceptionType)
                 .HasMaxLength(300)
                 .HasColumnType("varchar(300)");
                b.Property(e => e.Source)
                .HasMaxLength(300)
                .HasColumnType("varchar(300)");

                b.Property(e => e.UserId)
                 .HasMaxLength(100)
                 .HasColumnType("varchar(100)");

                b.Property(e => e.Message)
                 .HasColumnType("varchar(max)");       

                b.Property(e => e.StackTrace)
                 .HasColumnType("varchar(max)");       

                b.Property(e => e.CreatedAt)
                 .HasColumnType("datetime2")
                 .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
