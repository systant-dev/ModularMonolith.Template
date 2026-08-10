using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserModule.Storage;

internal sealed class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    #region Public Method Declarations

    public UserDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<UserDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(
            "Server=(localdb)\\mssqllocaldb;Database=ModularMonolith;Trusted_Connection=True;",
            sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", UserDbContext.SchemaName));

        return new UserDbContext(optionsBuilder.Options);
    }

    #endregion
}
