

using System.ComponentModel.DataAnnotations;

namespace ZurichAPI.Models.Request.Clients;

public class CreateClientRequest
{
    [Required, MaxLength(30)]
    public string Phone { get; set; }
}
