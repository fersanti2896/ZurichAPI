
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ZurichAPI.Data.SQL.Entities;

[Table("TClients")]
public class TClients : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClientId { get; set; }
    [Required]
    public int UserId { get; set; }

    [Required, MaxLength(10)]
    public long IdentificationNumber { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; }

    [Required, MaxLength(80)]
    public string LastName { get; set; }

    [Required, MaxLength(80)]
    public string SurName { get; set; }

    [Required, MaxLength(150)]
    public string Email { get; set; }

    [Required, MaxLength(30)]
    public string Phone { get; set; }

    public virtual TUsers? User { get; set; }
}
