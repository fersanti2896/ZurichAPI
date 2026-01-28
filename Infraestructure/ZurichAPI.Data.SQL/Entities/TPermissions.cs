
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZurichAPI.Data.SQL.Entities;

[Table("TPermissions")]
public class TPermissions : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PermissionId { get; set; }

    [Required, MaxLength(80)]
    public string Key { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(200)]
    public string? Description { get; set; }
}
