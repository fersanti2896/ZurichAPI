
using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Clients;

public class ClientsResponse : BaseResponse
{
    public List<ClientDTO>? Result { get; set; }
}
