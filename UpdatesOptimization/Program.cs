using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OptimizeMigrations.Contexts;
using OptimizeMigrations.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseHttpsRedirection();

//Minimal Api puth endpoint
app.MapPut("increase-salaries", async (int companyId, AppDbContext DbContext) =>
{
    var company = await DbContext.Set<Company>()
        .Include(c => c.Employees)
        .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
        return Results.NotFound($"The company with the id '{companyId}' was not found");

    foreach (var employee in company.Employees)
    {
        employee.Salary *= 1.1m;
    }

    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    await DbContext.SaveChangesAsync();
    return Results.NoContent();
});
//Diferencias: aqui en vez de realizarse 1001 operaciones de actualizado solo se hace una
app.MapPut("increase-salaries-best-practice", async (int companyId, AppDbContext dbContext) =>
{
    var company = await dbContext.Set<Company>()
        .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
        return Results.NotFound($"The company with the id '{companyId}' was not found");

    //Las transacciones las agregamos porque al ejecutar la linea 45, se actualiza la db sin necesidad del saveChanges
    //Por lo que al comenzar una transaccion, hasta que no se realice, respetara el saveChanges
    await dbContext.Database.BeginTransactionAsync();

    await dbContext.Database.ExecuteSqlInterpolatedAsync(
        $"UPDATE Employees SET Salary = Salary * 1.1 where CompanyId ={companyId}");

    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    await dbContext.SaveChangesAsync();

    await dbContext.Database.CommitTransactionAsync();

    return Results.NoContent();
});

app.MapPut("increase-salaries-dapper", async (int companyId, AppDbContext dbContext) =>
{
    var company = await dbContext.Set<Company>()
        .FirstOrDefaultAsync(c => c.Id == companyId);

    if (company is null)
        return Results.NotFound($"The company with the id '{companyId}' was not found");

    //Esto lo hacemos porque dapper no reconoce automaticamente las transacciones de EFCore, por lo que 
    //simplemente la agregamos al metodo de execute async para que la reconozca
    var transaction = await dbContext.Database.BeginTransactionAsync();

    await dbContext.Database.GetDbConnection().ExecuteAsync(
        "UPDATE Employees SET Salary = Salary * 1.1 WHERE CompanyId = @CompanyId",
        new {CompanyId = company.Id},
        transaction.GetDbTransaction());

    company.LastSalaryUpdateUtc = DateTime.UtcNow;
    await dbContext.SaveChangesAsync();

    await dbContext.Database.CommitTransactionAsync();

    return Results.NoContent();
});

app.Run();