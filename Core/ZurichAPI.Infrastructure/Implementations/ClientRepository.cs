
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
    private const string ClientsAllKey = "clients:all:status1";

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
            await Cache.RemoveAsync(ClientsAllKey);

        return result;
    }

    public async Task<ReplyResponse> UpdateMyProfile(UpdateMyProfileRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessClient.UpdateMyProfile(request, userId), "UpdateMyProfile", userId);

        if (result.Error == null)
            await Cache.RemoveAsync(ClientsAllKey);

        return result;
    }

    public async Task<ReplyResponse> UpdateClientByAdmin(UpdateClientRequest request, int userId)
    {
        var result = await ExecuteWithLogging(() => IDataAccessClient.UpdateClientByAdmin(request, userId), "UpdateClientByAdmin", userId);

        if (result.Error == null)
            await Cache.RemoveAsync(ClientsAllKey);

        return result;
    }

    public async Task<ClientsResponse> GetAllClients(int userId)
    {
        ClientsResponse response = new();

        try
        {
            var cached = await Cache.GetAsync<List<ClientDTO>>(ClientsAllKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessClient.GetAllClients(userId);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(ClientsAllKey, response.Result, TimeSpan.FromMinutes(2));

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
}
