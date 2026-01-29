using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Catalogs;

public class GetStatesResponse : BaseResponse
{
    public List<StatesDTO>? Result { get; set; }
}
