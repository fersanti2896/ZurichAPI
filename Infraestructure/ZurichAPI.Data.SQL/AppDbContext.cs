
using Microsoft.EntityFrameworkCore;

namespace ZurichAPI.Data.SQL;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
    {
    }
}
