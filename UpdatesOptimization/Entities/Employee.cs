namespace OptimizeMigrations.Entities;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Salary { get; set; }
    // Campo para que EFCore haga las relaciones => foreign key
    public int CompanyId { get; set; }
}