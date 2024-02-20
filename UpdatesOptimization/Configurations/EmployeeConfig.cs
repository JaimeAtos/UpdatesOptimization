using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OptimizeMigrations.Entities;

namespace OptimizeMigrations.Configurations;

public class EmployeeConfig : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        var employees = Enumerable.Range(1, 1000)
            .Select(id => new Employee
            {
                Id = id,
                Name = $"Employee #{id}",
                Salary = 100.0m,
                CompanyId = 1
            }).ToList();

        builder.HasData(employees);
    }
}