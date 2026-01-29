using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Catalogs;

public class PolicyTypesResponse : BaseResponse
{
    public List<PolicyTypesDTO>? Result { get; set; }
}
