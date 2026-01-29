using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZurichAPI.Data.SQL.Entities;

[Table("TPolicyTypes")]
public class TPolicyTypes : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PolicyTypeId { get; set; }
    public string Name { get; set; }
}
