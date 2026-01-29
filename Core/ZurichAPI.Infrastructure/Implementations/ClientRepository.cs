
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Clients;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Clients;

namespace ZurichAPI.Infrastructure.Implementations;

public class ClientRepository : IClientRepository
{
    private readonly IDataAccessClient IDataAccessClient;
    private IDataAccessLogs IDataAccessLogs;
    private readonly ICacheService Cache;
    private const string ClientsVersionKey = "clients:version";

    public ClientRepository(IDataAccessClient iDataAccessClient, IDataAccessLogs iDataAccessLogs, ICacheService cache)
    {
        IDataAccessClient = iDataAccessClient;
        IDataAccessLogs = iDataAccessLogs;
        Cache = cache;
    }

    public async Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessClient.CreateClient(request, userId), "CreateClient", userId);

        if (result.Error == null)
            await BumpClientsCacheVersionAsync();

        return result;
    }

    public async Task<ReplyResponse> UpdateMyProfile(UpdateMyProfileRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessClient.UpdateMyProfile(request, userId), "UpdateMyProfile", userId);

        if (result.Error == null)
            await BumpClientsCacheVersionAsync();

        return result;
    }

    public async Task<ReplyResponse> UpdateClientByAdmin(UpdateClientRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessClient.UpdateClientByAdmin(request, userId), "UpdateClientByAdmin", userId);

        if (result.Error == null)
            await BumpClientsCacheVersionAsync();

        return result;
    }

    public async Task<ClientsResponse> GetAllClients(GetClientsRequest request, int userId)
    {
        ClientsResponse response = new();

        try
        {
            var version = await GetClientsCacheVersionAsync();
            var cacheKey = BuildClientsKey(version, request);

            var cached = await Cache.GetAsync<List<ClientDTO>>(cacheKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessClient.GetAllClients(request, userId);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(cacheKey, response.Result, TimeSpan.FromMinutes(2));

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-ClientRepository",
                Action = "GetAllClients",
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

    public async Task<ReplyResponse> DeleteClient(DeleteClienteRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessClient.DeleteClient(request, userId), "DeleteClient", userId);

        if (result.Error == null)
            await BumpClientsCacheVersionAsync();
        
        return result;
    }

    #region Métodos privados

    private static string NormalizeLower(string? value)
        => string.IsNullOrWhiteSpace(value) ? "null" : value.Trim().ToLowerInvariant();

    private static string NormalizeUpper(string? value)
        => string.IsNullOrWhiteSpace(value) ? "null" : value.Trim().ToUpperInvariant();

    private static string BuildClientsKey(int version, GetClientsRequest r)
        => $"clients:v{version}:all:status1:" +
           $"name:{NormalizeLower(r.Name)}:" +
           $"email:{NormalizeLower(r.Email)}:" +
           $"id:{NormalizeUpper(r.IdentificationNumber)}";

    private async Task<int> GetClientsCacheVersionAsync()
    {
        var v = await Cache.GetAsync<int?>(ClientsVersionKey);

        if (v == null || v <= 0)
        {
            await Cache.SetAsync(ClientsVersionKey, 1, TimeSpan.FromDays(30));
            return 1;
        }

        return v.Value;
    }

    private async Task BumpClientsCacheVersionAsync()
    {
        var current = await GetClientsCacheVersionAsync();
        await Cache.SetAsync(ClientsVersionKey, current + 1, TimeSpan.FromDays(30));
    }

    private async Task<T> ExecuteWithLogging<T>(Func<Task<T>> action, string actionName, int userId) where T : BaseResponse, new()
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
                Module = "ZurichAPI-ClientRepository",
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
