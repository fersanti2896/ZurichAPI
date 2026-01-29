using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZurichAPI.Data.SQL.Entities;

[Table("TPolicyStatuses")]
public class TPolicyStatuses : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PolicyStatusId { get; set; }
    public string Name { get; set; }
}
