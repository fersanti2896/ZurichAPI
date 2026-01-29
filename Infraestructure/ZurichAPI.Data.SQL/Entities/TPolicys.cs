using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace ZurichAPI.Data.SQL.Entities;

[Table("TPolicys")]
public class TPolicys : TDataGeneric
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PolicyId { get; set; }
    public int ClientId { get; set; }
    public int PolicyTypeId { get; set; }
    public int PolicyStatusId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InsuredAmount { get; set; }

    [ForeignKey("ClientId")]
    public virtual TClients Clients { get; set; }

    [ForeignKey("PolicyTypeId")]
    public virtual TPolicyTypes PolicyTypes { get; set; }

    [ForeignKey("PolicyStatusId")]
    public virtual TPolicyStatuses PolicyStatuses { get; set; }
}
