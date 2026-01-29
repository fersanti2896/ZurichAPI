
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Infrastructure.Interfaces;

namespace ZurichAPI.Infrastructure.Implementations;

public class PolicyRepository : IPolicyRepository
{
    private readonly IDataAccessPolicy IDataAccessPolicy;
    private IDataAccessLogs IDataAccessLogs;
    private readonly ICacheService Cache;

    private const string ClientsAllKey = "clients:all:status1";

    public PolicyRepository(IDataAccessPolicy iDataAccessPolicy, IDataAccessLogs iDataAccessLogs, ICacheService cache)
    {
        IDataAccessPolicy = iDataAccessPolicy;
        IDataAccessLogs = iDataAccessLogs;
        Cache = cache;
    }
}
