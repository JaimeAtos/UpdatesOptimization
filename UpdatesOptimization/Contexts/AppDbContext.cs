using System.Reflection;
using Microsoft.EntityFrameworkCore;
using OptimizeMigrations.Entities;

namespace OptimizeMigrations.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Solamente configurare de esta forma el company, employee lo hare con configurations
        modelBuilder.Entity<Company>(builder =>
        {
            builder.ToTable("Companies");

            builder.HasMany(company => company.Employees)
                .WithOne()
                .HasForeignKey(employee => employee.CompanyId)
                .IsRequired();

            builder.HasData(new Company
            {
                Id = 1,
                Name = "Test Company"
            });
        });
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}