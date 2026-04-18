using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ApuestasDeportivas.Infrastructure.Persistence;

/// <summary>
/// Factory para que dotnet-ef pueda crear el DbContext durante migraciones.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=apuestasdeportivas.dev.db");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
