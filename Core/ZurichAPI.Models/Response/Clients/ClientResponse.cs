using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Clients;

public class ClientResponse : BaseResponse
{
    public ClientDTO? Result { get; set; }
}
