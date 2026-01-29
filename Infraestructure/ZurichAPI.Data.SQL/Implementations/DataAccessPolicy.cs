using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZurichAPI.Data.SQL.Entities;
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Policys;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Policys;

namespace ZurichAPI.Data.SQL.Implementations;

public class DataAccessPolicy : IDataAccessPolicy
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);
    private const int PolicyStatusActiveId = 1;
    private const int PolicyStatusCancelledId = 2;
    private const int PolicyStatusCancelRequestedId = 3;

    public DataAccessPolicy(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreatePolicy(CreatePolicyRequest request, int userId)
    {
        var response = new ReplyResponse();

        try { 

            if (request.EndDate <= request.StartDate)
                return SetError(response, 400, "La fecha de expiración debe ser posterior a la fecha de inicio.");
           
            if (request.InsuredAmount <= 0)
                return SetError(response, 400, "El monto asegurado debe ser positivo.");

            var clientExists = await Context.TClients.AnyAsync(x => x.ClientId == request.ClientId && x.Status == 1);

            if (!clientExists)
                return SetError(response, 400, "No se encontró el cliente.");
   
            var policyTypeExists = await Context.TPolicyTypes.AnyAsync(x => x.PolicyTypeId == request.PolicyTypeId && x.Status == 1);

            if (!policyTypeExists)
                return SetError(response, 400, "El tipo de póliza no es válido.");


            var entity = new TPolicys
            {
                ClientId = request.ClientId,
                PolicyTypeId = request.PolicyTypeId,
                PolicyStatusId = PolicyStatusActiveId,
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate.Date,
                InsuredAmount = request.InsuredAmount,
                Status = 1,
                CreateDate = DateTime.Now,
                CreateUser = userId
            };

            await Context.TPolicys.AddAsync(entity);

            var saved = await Context.SaveChangesAsync() > 0;
            if (!saved)
                return SetError(response, 500, "No fue posible crear la póliza.");

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Póliza creada"
            };

            return response;

        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessPolicy",
                Action = "CreatePolicy",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = $"Error Exception: {ex.InnerException?.Message ?? ex.Message}"
            };
        }

        return response;
    }

    public async Task<PolicysReponse> GetAllPolicys(GetPolicysRequest request, int userId)
    {
        PolicysReponse response = new();

        try
        {
            var query = from p in Context.TPolicys
                        join c in Context.TClients on p.ClientId equals c.ClientId
                        join pt in Context.TPolicyTypes on p.PolicyTypeId equals pt.PolicyTypeId
                        join ps in Context.TPolicyStatuses on p.PolicyStatusId equals ps.PolicyStatusId
                        where p.Status == 1 && c.Status == 1
                        select new
                        {
                            p.PolicyId,
                            p.ClientId,
                            c.Name,
                            c.LastName,
                            c.SurName,
                            p.PolicyTypeId,
                            PolicyName = pt.Name,
                            p.PolicyStatusId,
                            StatusName = ps.Name,
                            p.StartDate,
                            p.EndDate,
                            p.InsuredAmount
                        };

            // Filtro por rango de fechas
            if (request.StartDate.HasValue)
            {
                var start = request.StartDate.Value.Date;
                query = query.Where(x => x.StartDate >= start);
            }

            if (request.EndDate.HasValue)
            {
                var end = request.EndDate.Value.Date;
                query = query.Where(x => x.EndDate <= end);
            }

            // Filtro por tipo
            if (request.PolicyTypeId.HasValue && request.PolicyTypeId.Value > 0)
                query = query.Where(x => x.PolicyTypeId == request.PolicyTypeId.Value);

            // Filtro por estatus
            if (request.PolicyStatusId.HasValue && request.PolicyStatusId.Value > 0)
                query = query.Where(x => x.PolicyStatusId == request.PolicyStatusId.Value);

            var raw = await query.ToListAsync();

            response.Result = raw.Select(x => new PolicyDTO
                                {
                                    PolicyId = x.PolicyId,
                                    ClientId = x.ClientId,
                                    FullName = $"{x.Name} {x.LastName} {x.SurName}".Replace("  ", " ").Trim(),
                                    PolicyTypeId = x.PolicyTypeId,
                                    PolicyName = x.PolicyName,
                                    PolicyStatusId = x.PolicyStatusId,
                                    StatusName = x.StatusName,
                                    StartDate = x.StartDate,
                                    EndDate = x.EndDate,
                                    InsuredAmount = x.InsuredAmount
                                })
                                .OrderBy(x => x.FullName)
                                .ThenBy(x => x.PolicyName)
                                .ToList();
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessPolicy",
                Action = "GetAllPolicys",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }

        return response;
    }

    public async Task<PolicysReponse> GetMyPolicys(int userId)
    {
        PolicysReponse response = new();

        try
        {
            var clientId = await Context.TClients
                .Where(x => x.UserId == userId && x.Status == 1)
                .Select(x => x.ClientId)
                .FirstOrDefaultAsync();

            if (clientId == 0)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "No se encontró un cliente asociado al usuario."
                };
                return response;
            }

            var raw = await (from p in Context.TPolicys
                            join c in Context.TClients on p.ClientId equals c.ClientId
                            join pt in Context.TPolicyTypes on p.PolicyTypeId equals pt.PolicyTypeId
                            join ps in Context.TPolicyStatuses on p.PolicyStatusId equals ps.PolicyStatusId
                            where p.PolicyStatusId != 3
                                  && c.Status == 1
                                  && p.ClientId == clientId
                            select new
                            {
                                p.PolicyId,
                                p.ClientId,
                                c.Name,
                                c.LastName,
                                c.SurName,
                                p.PolicyTypeId,
                                PolicyName = pt.Name,
                                p.PolicyStatusId,
                                StatusName = ps.Name,
                                p.StartDate,
                                p.EndDate,
                                p.InsuredAmount
                            }
                        ).ToListAsync();

            response.Result = raw
                .Select(x => new PolicyDTO
                {
                    PolicyId = x.PolicyId,
                    ClientId = x.ClientId,
                    FullName = $"{x.Name} {x.LastName} {x.SurName}".Replace("  ", " ").Trim(),
                    PolicyTypeId = x.PolicyTypeId,
                    PolicyName = x.PolicyName,
                    PolicyStatusId = x.PolicyStatusId,
                    StatusName = x.StatusName,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    InsuredAmount = x.InsuredAmount
                })
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.EndDate)
                .ToList();
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessPolicy",
                Action = "GetMyPolicys",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.InnerException?.Message ?? ex.Message
            };
        }

        return response;
    }

    public async Task<ReplyResponse> RequestCancelPolicy(CancelPolicyRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            if (request.PolicyId <= 0)
                return SetError(response, 400, "La póliza no es válida.");

            var clientId = await Context.TClients
                .Where(x => x.UserId == userId && x.Status == 1)
                .Select(x => x.ClientId)
                .FirstOrDefaultAsync();

            if (clientId == 0)
                return SetError(response, 404, "No se encontró un cliente asociado al usuario.");

            var policy = await Context.TPolicys.FirstOrDefaultAsync(x => x.PolicyId == request.PolicyId);

            if (policy == null)
                return SetError(response, 404, "No se encontró la póliza.");

            if (policy.ClientId != clientId)
                return SetError(response, 403, "No tienes permisos para cancelar esta póliza.");

            if (policy.PolicyStatusId == PolicyStatusCancelledId)
                return SetError(response, 400, "La póliza ya se encuentra cancelada.");

            if (policy.PolicyStatusId == PolicyStatusCancelRequestedId)
                return SetError(response, 400, "La póliza ya tiene una cancelación solicitada.");

            if (policy.PolicyStatusId != PolicyStatusActiveId)
                return SetError(response, 400, "Solo se puede solicitar cancelación para pólizas activas.");


            policy.PolicyStatusId = PolicyStatusCancelRequestedId;
            policy.UpdateDate = NowCDMX;
            policy.UpdateUser = userId;

            var saved = await Context.SaveChangesAsync() > 0;
            if (!saved)
                return SetError(response, 500, "No fue posible solicitar la cancelación de la póliza.");

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Cancelación solicitada correctamente."
            };

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessPolicy",
                Action = "RequestCancelPolicy",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.InnerException?.Message ?? ex.Message
            };

            return response;
        }
    }

    public async Task<ReplyResponse> ApproveCancelPolicy(CancelPolicyRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            if (request.PolicyId <= 0)
                return SetError(response, 400, "La póliza no es válida.");

            var policy = await Context.TPolicys.FirstOrDefaultAsync(x => x.PolicyId == request.PolicyId && x.Status == 1);

            if (policy == null)
                return SetError(response, 404, "No se encontró la póliza.");

            if (policy.PolicyStatusId == PolicyStatusCancelledId)
                return SetError(response, 400, "La póliza ya se encuentra cancelada.");

            if (policy.PolicyStatusId != PolicyStatusCancelRequestedId)
                return SetError(response, 400, "La póliza no tiene una cancelación solicitada.");

            policy.PolicyStatusId = PolicyStatusCancelledId;
            policy.UpdateDate = NowCDMX;
            policy.UpdateUser = userId;

            var saved = await Context.SaveChangesAsync() > 0;
            if (!saved)
                return SetError(response, 500, "No fue posible cancelar la póliza.");

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Póliza cancelada correctamente."
            };

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessPolicy",
                Action = "ApproveCancelPolicy",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = ex.InnerException?.Message ?? ex.Message
            };

            return response;
        }
    }

    public async Task<int> GetUserIdByClientId(int clientId)
    {
        return await Context.TClients
                            .Where(x => x.ClientId == clientId && x.Status == 1)
                            .Select(x => x.UserId)
                            .FirstOrDefaultAsync();
    }

    public async Task<int> GetClientIdByPolicyId(int policyId)
    {
        return await Context.TPolicys
            .Where(x => x.PolicyId == policyId && x.Status == 1)
            .Select(x => x.ClientId)
            .FirstOrDefaultAsync();
    }

    private static ReplyResponse SetError(ReplyResponse response, int code, string message)
    {
        response.Error = new ErrorDTO
        {
            Code = code,
            Message = message
        };

        return response;
    }
}
