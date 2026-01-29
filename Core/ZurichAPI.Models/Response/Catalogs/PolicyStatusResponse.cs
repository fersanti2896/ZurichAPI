using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Catalogs;

public class PolicyStatusResponse : BaseResponse
{
    public List<PolicyStatusesDTO>? Result { get; set; }
}
