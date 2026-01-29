
using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Policys;

public class PolicysReponse : BaseResponse
{
    public List<PolicyDTO>? Result { get; set; }
}
