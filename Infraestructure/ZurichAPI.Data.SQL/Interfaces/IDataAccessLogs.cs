using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Data.SQL.Interfaces;

public interface IDataAccessLogs
{
    Task<bool> Create(LogsDTO logDTO);
}
