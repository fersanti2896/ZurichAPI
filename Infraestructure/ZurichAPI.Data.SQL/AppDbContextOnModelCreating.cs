using Microsoft.EntityFrameworkCore;

namespace ZurichAPI.Data.SQL;

public partial class AppDbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
