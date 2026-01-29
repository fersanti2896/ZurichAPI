
using ZurichAPI.Models.Request.Policys;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Policys;

namespace ZurichAPI.Infrastructure.Interfaces;

public interface IPolicyRepository
{
    Task<ReplyResponse> CreatePolicy(CreatePolicyRequest request, int userId);
    Task<PolicysReponse> GetAllPolicys(GetPolicysRequest request, int userId);
    Task<PolicysReponse> GetMyPolicys(int userId);
    Task<ReplyResponse> RequestCancelPolicy(CancelPolicyRequest request, int userId);
    Task<ReplyResponse> ApproveCancelPolicy(CancelPolicyRequest request, int userId);
}
