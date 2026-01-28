
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Response;

namespace ZurichAPI.Infrastructure.Implementations;

public class ClientRepository : IClientRepository
{
    private readonly IDataAccessClient IDataAccessClient;
    private IDataAccessLogs IDataAccessLogs;

    public ClientRepository(IDataAccessClient iDataAccessClient, IDataAccessLogs iDataAccessLogs)
    {
        IDataAccessClient = iDataAccessClient;
        IDataAccessLogs = iDataAccessLogs;
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
