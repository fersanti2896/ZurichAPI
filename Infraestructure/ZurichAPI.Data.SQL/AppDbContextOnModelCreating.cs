using Microsoft.EntityFrameworkCore;
using ZurichAPI.Data.SQL.Entities;

namespace ZurichAPI.Data.SQL;

public partial class AppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TRolePermissions>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        base.OnModelCreating(modelBuilder);
    }

}
