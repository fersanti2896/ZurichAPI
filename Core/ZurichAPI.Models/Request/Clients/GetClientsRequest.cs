

namespace ZurichAPI.Models.Request.Clients;

public class GetClientsRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? IdentificationNumber { get; set; }
}
