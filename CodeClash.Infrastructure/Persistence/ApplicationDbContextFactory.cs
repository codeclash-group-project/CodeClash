using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CodeClash.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF Core tooling (migrations).
/// Uses a dummy connection string — actual connection is supplied via appsettings at runtime.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        // Use a placeholder connection string for migrations only.
        // The real connection string is read from appsettings.json at runtime.
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\MSSQLLocalDB;Database=CodeClash_Design;Trusted_Connection=True;",
            b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
