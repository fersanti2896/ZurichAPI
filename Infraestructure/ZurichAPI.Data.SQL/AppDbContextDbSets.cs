using Microsoft.EntityFrameworkCore;
using ZurichAPI.Data.SQL.Entities;

namespace ZurichAPI.Data.SQL;

public partial class AppDbContext
{
    public virtual DbSet<TLog> TLogs { get; set; }
    public virtual DbSet<TUsers> TUsers { get; set; }
    public virtual DbSet<TRol> TRol { get; set; }
    public virtual DbSet<TRefreshTokens> TRefreshTokens { get; set; }
    public virtual DbSet<TPermissions> TPermissions { get; set; }
    public virtual DbSet<TRolePermissions> TRolePermissions { get; set; }
    public virtual DbSet<TClients> TClients { get; set; }
    public virtual DbSet<TPostalCodes> TPostalCodes { get; set; }
    public virtual DbSet<TClientsAddress> TClientsAddress { get; set; }
    public virtual DbSet<TPolicyStatuses> TPolicyStatuses { get; set; }
    public virtual DbSet<TPolicyTypes> TPolicyTypes { get; set; }
    public virtual DbSet<TPolicys> TPolicys { get; set; }
}
