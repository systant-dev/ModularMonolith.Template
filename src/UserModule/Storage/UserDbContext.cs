using Microsoft.EntityFrameworkCore;
using ModularMonolith.Template.SharedKernel.Persistence;
using UserModule.Models;

namespace UserModule.Storage;

internal sealed class UserDbContext(DbContextOptions<UserDbContext> options) : ModuleDbContext(options)
{
    #region Constant Declarations

    public const string SchemaName = "user";

    #endregion

    #region Property Declarations

    public DbSet<User> Users => Set<User>();

    protected override string Schema => SchemaName;

    #endregion

    #region Protected Method Declarations

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        base.OnModelCreating(modelBuilder);
    }

    #endregion
}
