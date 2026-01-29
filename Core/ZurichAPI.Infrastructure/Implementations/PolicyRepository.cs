
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Policys;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Policys;

namespace ZurichAPI.Infrastructure.Implementations;

public class PolicyRepository : IPolicyRepository
{
    private readonly IDataAccessPolicy IDataAccessPolicy;
    private IDataAccessLogs IDataAccessLogs;
    private readonly ICacheService Cache;
    private const string PolicysVersionKey = "policys:version";
    private static string PoliciesByClientKey(int clientId) => $"policies:client:{clientId}:all";
    private static string PoliciesByClientActiveKey(int clientId) => $"policies:client:{clientId}:active";
    private static string MyPoliciesKey(int userId) => $"policies:me:user:{userId}:all";

    public PolicyRepository(IDataAccessPolicy iDataAccessPolicy, IDataAccessLogs iDataAccessLogs, ICacheService cache)
    {
        IDataAccessPolicy = iDataAccessPolicy;
        IDataAccessLogs = iDataAccessLogs;
        Cache = cache;
    }

    public async Task<ReplyResponse> CreatePolicy(CreatePolicyRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessPolicy.CreatePolicy(request, userId), "CreatePolicy", userId);

        if (result.Error == null)
        {
            await Cache.RemoveAsync(PoliciesByClientKey(request.ClientId));
            await Cache.RemoveAsync(PoliciesByClientActiveKey(request.ClientId));

            var clientUserId = await IDataAccessPolicy.GetUserIdByClientId(request.ClientId);
            if (clientUserId > 0)
                await Cache.RemoveAsync(MyPoliciesKey(clientUserId));

            await BumpPolicysCacheVersionAsync();
        }

        return result;
    }

    public async Task<PolicysReponse> GetAllPolicys(GetPolicysRequest request, int userId)
    {
        PolicysReponse response = new();

        try
        {
            var version = await GetPolicysCacheVersionAsync();
            var cacheKey = BuildPolicysKey(version, request);

            var cached = await Cache.GetAsync<List<PolicyDTO>>(cacheKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessPolicy.GetAllPolicys(request, userId);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(cacheKey, response.Result, TimeSpan.FromMinutes(2));

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-PolicyRepository",
                Action = "GetAllPolicys",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };

            return response;
        }
    }

    public async Task<PolicysReponse> GetMyPolicys(int userId)
    {
        PolicysReponse response = new();

        try
        {
            var cacheKey = MyPoliciesKey(userId);

            var cached = await Cache.GetAsync<List<PolicyDTO>>(cacheKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessPolicy.GetMyPolicys(userId);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(cacheKey, response.Result, TimeSpan.FromMinutes(2));

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-PolicyRepository",
                Action = "GetMyPolicys",
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };

            return response;
        }
    }

    public async Task<ReplyResponse> RequestCancelPolicy(CancelPolicyRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessPolicy.RequestCancelPolicy(request, userId), "RequestCancelPolicy", userId);

        if (result.Error == null)
        {
            await Cache.RemoveAsync(MyPoliciesKey(userId));

            await BumpPolicysCacheVersionAsync();
        }

        return result;
    }

    public async Task<ReplyResponse> ApproveCancelPolicy(CancelPolicyRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessPolicy.ApproveCancelPolicy(request, userId), "ApproveCancelPolicy", userId);

        if (result.Error == null)
        {
            var clientId = await IDataAccessPolicy.GetClientIdByPolicyId(request.PolicyId);
            if (clientId > 0)
            {
                await Cache.RemoveAsync(PoliciesByClientKey(clientId));
                await Cache.RemoveAsync(PoliciesByClientActiveKey(clientId));

                var ownerUserId = await IDataAccessPolicy.GetUserIdByClientId(clientId);
                if (ownerUserId > 0)
                    await Cache.RemoveAsync(MyPoliciesKey(ownerUserId));
            }

            await BumpPolicysCacheVersionAsync();
        }

        return result;
    }

    #region Metodos privados
    private static string NormalizeDate(DateTime? dt)
        => dt == null ? "null" : dt.Value.Date.ToString("yyyyMMdd");

    private static string NormalizeInt(int? v)
        => v == null ? "null" : v.Value.ToString();

    private static string BuildPolicysKey(int version, GetPolicysRequest r)
        => $"policys:v{version}:all:" +
           $"start:{NormalizeDate(r.StartDate)}:" +
           $"end:{NormalizeDate(r.EndDate)}:" +
           $"type:{NormalizeInt(r.PolicyTypeId)}:" +
           $"status:{NormalizeInt(r.PolicyStatusId)}";

    private async Task<int> GetPolicysCacheVersionAsync()
    {
        var v = await Cache.GetAsync<int?>(PolicysVersionKey);

        if (v == null || v <= 0)
        {
            await Cache.SetAsync(PolicysVersionKey, 1, TimeSpan.FromDays(30));
            return 1;
        }

        return v.Value;
    }

    private async Task BumpPolicysCacheVersionAsync()
    {
        var current = await GetPolicysCacheVersionAsync();
        await Cache.SetAsync(PolicysVersionKey, current + 1, TimeSpan.FromDays(30));
    }

    private async Task<T> ExecuteWithLogging<T>(Func<Task<T>> action, string actionName, int userId)
        where T : BaseResponse, new()
    {
        T response = new();

        try
        {
            response = await action();
            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-PolicyRepository",
                Action = actionName,
                Message = $"Exception: {ex.Message}",
                InnerException = $"InnerException: {ex.InnerException?.Message}"
            };
            await IDataAccessLogs.Create(log);

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.Message
            };

            return response;
        }
    }
    #endregion
}
