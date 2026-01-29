using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Catalogs;
using ZurichAPI.Models.Response.Catalogs;
using ZurichAPI.Models.Response;

namespace ZurichAPI.Infrastructure.Implementations;

public class CatalogsRepository : ICatalogsRepository
{
    private readonly IDataAccessCatalogs IDataAccessCatalogs;
    private IDataAccessLogs IDataAccessLogs;
    private readonly ICacheService Cache;
    private const string StatesAllKey = "states:all:status1";
    private const string PolicyTypesAllKey = "policytypes:all:status1";
    private const string PolicyStatusAllKey = "policystatus:all:status1";

    public CatalogsRepository(IDataAccessCatalogs iDataAccessCatalogs, IDataAccessLogs iDataAccessLogs, ICacheService cache)
    {
        IDataAccessCatalogs = iDataAccessCatalogs;
        IDataAccessLogs = iDataAccessLogs;
        Cache = cache;
    }

    public async Task<MunicipalityByStateResponse> GetMunicipalityByState(MunicipalityByStateRequest request, int IdUser)
    {
        return await ExecuteWithLogging(() => IDataAccessCatalogs.GetMunicipalityByState(request, IdUser), "GetMunicipalityByState", IdUser);
    }

    public async Task<TownByStateAndMunicipalityResponse> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request, int IdUser)
    {
        return await ExecuteWithLogging(() => IDataAccessCatalogs.GetTownByStateAndMunicipality(request, IdUser), "GetTownByStateAndMunicipality", IdUser);
    }

    public async Task<CPResponse> GetCP(CPRequest request, int IdUser)
    {
        return await ExecuteWithLogging(() => IDataAccessCatalogs.GetCP(request, IdUser), "GetCP", IdUser);
    }

    public async Task<GetStatesResponse> GetStates(int IdUser)
    {
        GetStatesResponse response = new();

        try
        {
            var cached = await Cache.GetAsync<List<StatesDTO>>(StatesAllKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessCatalogs.GetStates(IdUser);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(StatesAllKey, response.Result, TimeSpan.FromMinutes(2));

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "ZurichAPI-CatalogsRepository",
                Action = "GetStates",
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

    public async Task<PolicyTypesResponse> GetPolicyTypes(int IdUser)
    {
        PolicyTypesResponse response = new();

        try
        {
            var cached = await Cache.GetAsync<List<PolicyTypesDTO>>(PolicyTypesAllKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessCatalogs.GetPolicyTypes(IdUser);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(PolicyTypesAllKey, response.Result, TimeSpan.FromMinutes(2));

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "ZurichAPI-CatalogsRepository",
                Action = "GetPolicyTypes",
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

    public async Task<PolicyStatusResponse> GetPolicyStatus(int IdUser)
    {
        PolicyStatusResponse response = new();

        try
        {
            var cached = await Cache.GetAsync<List<PolicyStatusesDTO>>(PolicyStatusAllKey);
            if (cached != null)
            {
                response.Result = cached;
                return response;
            }

            response = await IDataAccessCatalogs.GetPolicyStatus(IdUser);

            if (response.Error == null && response.Result != null)
                await Cache.SetAsync(PolicyStatusAllKey, response.Result, TimeSpan.FromMinutes(2));

            return response;
        }
        catch (Exception ex)
        {
            var log = new LogsDTO
            {
                IdUser = 1,
                Module = "ZurichAPI-CatalogsRepository",
                Action = "GetPolicyTypes",
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
                Module = "ZurichAPI-CatalogsRepository",
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
