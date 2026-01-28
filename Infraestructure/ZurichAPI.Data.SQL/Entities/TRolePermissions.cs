
using System.ComponentModel.DataAnnotations.Schema;
using System.Security;

namespace ZurichAPI.Data.SQL.Entities;

[Table("TRolePermissions")]
public class TRolePermissions
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    public virtual TRol? Role { get; set; }
    public virtual TPermissions? Permission { get; set; }
}
