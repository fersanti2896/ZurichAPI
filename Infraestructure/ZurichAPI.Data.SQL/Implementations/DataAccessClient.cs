
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ZurichAPI.Data.SQL.Entities;
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Helpers;
using ZurichAPI.Models.Request.Clients;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Clients;

namespace ZurichAPI.Data.SQL.Implementations;

public class DataAccessClient : IDataAccessClient
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

    public DataAccessClient(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId)
    {
        ReplyResponse response = new();

        try
        {
            var clientRoleId = await Context.TRol
                .Where(r => r.Name == "Client" && r.Status == 1)
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();

            if (clientRoleId == 0)
                return SetError(response, 500, "No existe el rol 'Client'");

            var emailNormalized = request.Email.Trim().ToUpper();
            var emailExists = await Context.TUsers.AnyAsync(u => u.Email.ToUpper() == emailNormalized);

            if (emailExists)
                return SetError(response, 400, "Ya existe un usuario con el mismo correo.");

            using var tx = await Context.Database.BeginTransactionAsync();

            var passwordNormalized = request.Password.Trim().ToUpper();
            var encryptedPassword = EncryptDecrypt.EncryptString(passwordNormalized);

            var user = new TUsers
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                MLastName = request.MLastName.Trim() ?? string.Empty,
                Email = request.Email.Trim(),
                PasswordHash = encryptedPassword,
                RoleId = clientRoleId,
                Status = 1,
                CreateDate = NowCDMX,
                CreateUser = userId
            };

            await Context.TUsers.AddAsync(user);
            await Context.SaveChangesAsync();

            var identificationNumber = await GenerateUniqueIdentificationNumberAsync();

            var client = new TClients
            {
                UserId = user.UserId,
                IdentificationNumber = identificationNumber,
                Name = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                SurName = request.MLastName.Trim() ?? string.Empty,
                Email = request.Email.Trim(),
                Phone = request.Phone.Trim(),
                Status = 1,
                CreateDate = NowCDMX,
                CreateUser = userId
            };

            await Context.TClients.AddAsync(client);
            await Context.SaveChangesAsync(); 


            var address = new TClientsAddress
            {
                ClientId = client.ClientId,
                Cve_CodigoPostal = request.Cve_CodigoPostal.Trim(),
                Cve_Estado = request.Cve_Estado.Trim(),
                Cve_Municipio = request.Cve_Municipio.Trim(),
                Cve_Colonia = request.Cve_Colonia.Trim(),
                Street = request.Street.Trim(),
                ExtNbr = request.ExtNbr.Trim(),
                InnerNbr = request.InnerNbr?.Trim(),
                Status = 1,
                CreateDate = NowCDMX,
                CreateUser = userId
            };

            await Context.TClientsAddress.AddAsync(address);
            await Context.SaveChangesAsync();

            await tx.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Cliente creado"
            };

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = 1,
                Module = "ZurichAPI-DataAccessUser",
                Action = "CreateClient",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Ocurrió un error al procesar la solicitud."
            };

            return response;
        }
    }

    public async Task<ReplyResponse> UpdateMyProfile(UpdateMyProfileRequest request, int userId)
    {
        ReplyResponse response = new();
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var client = await Context.TClients
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Status == 1);

            if (client == null)
                return SetError(response, 404, "Cliente no encontrado.");

            client.Phone = request.Phone.Trim();
            client.UpdateDate = NowCDMX;
            client.UpdateUser = userId;

            var address = await Context.TClientsAddress
                .FirstOrDefaultAsync(a => a.ClientId == client.ClientId && a.Status == 1);

            if (address != null)
            {
                address.Cve_CodigoPostal = request.Cve_CodigoPostal.Trim();
                address.Cve_Estado = request.Cve_Estado.Trim();
                address.Cve_Municipio = request.Cve_Municipio.Trim();
                address.Cve_Colonia = request.Cve_Colonia.Trim();
                address.Street = request.Street.Trim();
                address.ExtNbr = request.ExtNbr.Trim();
                address.InnerNbr = request.InnerNbr?.Trim();
                address.UpdateDate = NowCDMX;
                address.UpdateUser = userId;
            }
            else
            {
                var newAddress = new TClientsAddress
                {
                    ClientId = client.ClientId,
                    Cve_CodigoPostal = request.Cve_CodigoPostal.Trim(),
                    Cve_Estado = request.Cve_Estado.Trim(),
                    Cve_Municipio = request.Cve_Municipio.Trim(),
                    Cve_Colonia = request.Cve_Colonia.Trim(),
                    Street = request.Street.Trim(),
                    ExtNbr = request.ExtNbr.Trim(),
                    InnerNbr = request.InnerNbr?.Trim(),
                    Status = 1,
                    CreateDate = NowCDMX,
                    CreateUser = userId
                };

                await Context.TClientsAddress.AddAsync(newAddress);
            }

            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Perfil actualizado correctamente."
            };

            return response;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessUser",
                Action = "UpdateMyProfile",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Ocurrió un error al procesar la solicitud."
            };

            return response;
        }
    }

    public async Task<ReplyResponse> UpdateClientByAdmin(UpdateClientRequest request, int userId)
    {
        ReplyResponse response = new();
        using var tx = await Context.Database.BeginTransactionAsync();

        try
        {
            var client = await Context.TClients
                .FirstOrDefaultAsync(c => c.ClientId == request.ClientId && c.Status == 1);

            if (client == null)
                return SetError(response, 404, "Cliente no encontrado.");

            var user = await Context.TUsers
                .FirstOrDefaultAsync(u => u.UserId == client.UserId && u.Status == 1);

            if (user == null)
                return SetError(response, 404, "Usuario asociado al cliente no encontrado.");

            // Información del Cliente
            client.Name = request.FirstName.Trim();
            client.LastName = request.LastName.Trim();
            client.SurName = request.MLastName?.Trim() ?? string.Empty;
            client.Phone = request.Phone.Trim();
            client.UpdateDate = NowCDMX;
            client.UpdateUser = userId;

            // Información del Usuario
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.MLastName = request.MLastName?.Trim() ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var passwordNormalized = request.Password.Trim().ToUpper();
                user.PasswordHash = EncryptDecrypt.EncryptString(passwordNormalized);
            }

            user.UpdateDate = NowCDMX;
            user.UpdateUser = userId;

            // Edita la dirección del cliente
            var address = await Context.TClientsAddress
                .FirstOrDefaultAsync(a => a.ClientId == client.ClientId && a.Status == 1);

            if (address != null)
            {
                address.Cve_CodigoPostal = request.Cve_CodigoPostal.Trim();
                address.Cve_Estado = request.Cve_Estado.Trim();
                address.Cve_Municipio = request.Cve_Municipio.Trim();
                address.Cve_Colonia = request.Cve_Colonia.Trim();
                address.Street = request.Street.Trim();
                address.ExtNbr = request.ExtNbr.Trim();
                address.InnerNbr = request.InnerNbr?.Trim();
                address.UpdateDate = NowCDMX;
                address.UpdateUser = userId;
            }
            else
            {
                var newAddress = new TClientsAddress
                {
                    ClientId = client.ClientId,
                    Cve_CodigoPostal = request.Cve_CodigoPostal.Trim(),
                    Cve_Estado = request.Cve_Estado.Trim(),
                    Cve_Municipio = request.Cve_Municipio.Trim(),
                    Cve_Colonia = request.Cve_Colonia.Trim(),
                    Street = request.Street.Trim(),
                    ExtNbr = request.ExtNbr.Trim(),
                    InnerNbr = request.InnerNbr?.Trim(),
                    Status = 1,
                    CreateDate = NowCDMX,
                    CreateUser = userId
                };

                await Context.TClientsAddress.AddAsync(newAddress);
            }

            await Context.SaveChangesAsync();
            await tx.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Cliente actualizado correctamente."
            };

            return response;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();

            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessClient",
                Action = "UpdateClientByAdmin",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Ocurrió un error al procesar la solicitud."
            };

            return response;
        }
    }

    public async Task<ClientsResponse> GetAllClients(GetClientsRequest request, int userId)
    {
        ClientsResponse response = new();

        try
        {
            var query =
                from c in Context.TClients
                join ca in Context.TClientsAddress on c.ClientId equals ca.ClientId into addr
                from ca in addr.DefaultIfEmpty()
                join pc in Context.TPostalCodes on
                    new { ca.Cve_CodigoPostal, ca.Cve_Estado, ca.Cve_Municipio, ca.Cve_Colonia }
                    equals new
                    {
                        Cve_CodigoPostal = pc.d_codigo,
                        Cve_Estado = pc.c_estado,
                        Cve_Municipio = pc.c_mnpio,
                        Cve_Colonia = pc.id_asenta_cpcons
                    } into postal
                from pc in postal.DefaultIfEmpty()
                where c.Status == 1
                select new
                {
                    c.ClientId,
                    c.Name,
                    c.LastName,
                    c.SurName,
                    c.Email,
                    c.IdentificationNumber,
                    Phone = c.Phone,
                    c.Status,

                    Cve_CodigoPostal = ca != null ? ca.Cve_CodigoPostal : null,
                    Cve_Estado = ca != null ? ca.Cve_Estado : null,
                    Cve_Municipio = ca != null ? ca.Cve_Municipio : null,
                    Cve_Colonia = ca != null ? ca.Cve_Colonia : null,
                    Street = ca != null ? ca.Street : null,
                    ExtNbr = ca != null ? ca.ExtNbr : null,
                    InnerNbr = ca != null ? ca.InnerNbr : null,

                    d_asenta = pc != null ? pc.d_asenta : null,
                    D_mnpio = pc != null ? pc.D_mnpio : null,
                    d_estado = pc != null ? pc.d_estado : null,
                    d_codigo = pc != null ? pc.d_codigo : null
                };

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                var search = request.Name.Trim().ToLower();

                query = query.Where(x => (
                                            ((x.Name ?? "") + " " + (x.LastName ?? "") + " " + (x.SurName ?? ""))
                                            .ToLower()
                                         ).Contains(search)
                                    );
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                query = query.Where(x => x.Email.Contains(request.Email));
            }

            if (!string.IsNullOrWhiteSpace(request.IdentificationNumber))
            {
                long ident = long.Parse(request.IdentificationNumber);

                query = query.Where(x => x.IdentificationNumber == ident);
            }

            var raw = await query.ToListAsync();

            response.Result = raw
                .Select(x => new ClientDTO
                {
                    ClientId = x.ClientId,
                    FullName = $"{x.Name} {x.LastName} {x.SurName}".Replace("  ", " ").Trim(),
                    Email = x.Email,
                    PhoneNumber = x.Phone,
                    Status = x.Status,
                    IdentificationNumber = x.IdentificationNumber,
                    Cve_CodigoPostal = x.Cve_CodigoPostal ?? string.Empty,
                    Cve_Estado = x.Cve_Estado ?? string.Empty,
                    Cve_Municipio = x.Cve_Municipio ?? string.Empty,
                    Cve_Colonia = x.Cve_Colonia ?? string.Empty,
                    Street = x.Street ?? string.Empty,
                    ExtNbr = x.ExtNbr ?? string.Empty,
                    InnerNbr = x.InnerNbr ?? string.Empty,

                    Address = x.Street != null
                        ? $"{x.Street} #{x.ExtNbr}, {x.d_asenta}, {x.D_mnpio}, {x.d_estado}, CP {x.d_codigo}"
                        : null
                })
                .OrderBy(x => x.FullName)
                .ToList();
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessClient",
                Action = "GetAllClients",
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

    public async Task<ReplyResponse> DeleteClient(DeleteClienteRequest request, int userId)
    {
        var response = new ReplyResponse();

        try
        {
            if (request.ClientId <= 0)
                return SetError(response, 400, "El cliente no es válido.");

            var client = await Context.TClients.FirstOrDefaultAsync(x => x.ClientId == request.ClientId && x.Status == 1);

            if (client == null)
                return SetError(response, 404, "No se encontró el cliente o ya está inactivo.");

            // Se valida pólizas activas o con cancelación solicitada
            var hasActiveOrPendingPolicies = await Context.TPolicys.AnyAsync(p => p.ClientId == request.ClientId
                                                                             && p.Status == 1
                                                                             && (p.PolicyStatusId == 1 || p.PolicyStatusId == 3));

            if (hasActiveOrPendingPolicies)
                return SetError(response, 409, "No se puede eliminar el cliente porque tiene pólizas activas o con cancelación solicitada.");

            client.Status = 0;
            client.UpdateDate = NowCDMX;
            client.UpdateUser = userId;

            var saved = await Context.SaveChangesAsync() > 0;
            if (!saved)
                return SetError(response, 500, "No fue posible eliminar el cliente.");

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Cliente eliminado correctamente."
            };

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessClient",
                Action = "DeleteClient",
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

    public async Task<ClientResponse> GetMyClientProfile(int userId)
    {
        ClientResponse response = new();

        try
        {

            var clientId = await Context.TClients
                .Where(x => x.UserId == userId && x.Status == 1)
                .Select(x => x.ClientId)
                .FirstOrDefaultAsync();

            if (clientId <= 0)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "No se encontró un cliente."
                };
                return response;
            }

            var query =
                from c in Context.TClients
                join ca in Context.TClientsAddress on c.ClientId equals ca.ClientId into addr
                from ca in addr.DefaultIfEmpty()
                join pc in Context.TPostalCodes on
                    new { ca.Cve_CodigoPostal, ca.Cve_Estado, ca.Cve_Municipio, ca.Cve_Colonia }
                    equals new
                    {
                        Cve_CodigoPostal = pc.d_codigo,
                        Cve_Estado = pc.c_estado,
                        Cve_Municipio = pc.c_mnpio,
                        Cve_Colonia = pc.id_asenta_cpcons
                    } into postal
                from pc in postal.DefaultIfEmpty()
                where c.Status == 1 && c.ClientId == clientId
                select new
                {
                    c.ClientId,
                    c.Name,
                    c.LastName,
                    c.SurName,
                    c.Email,
                    c.IdentificationNumber,
                    Phone = c.Phone,
                    c.Status,

                    Cve_CodigoPostal = ca != null ? ca.Cve_CodigoPostal : null,
                    Cve_Estado = ca != null ? ca.Cve_Estado : null,
                    Cve_Municipio = ca != null ? ca.Cve_Municipio : null,
                    Cve_Colonia = ca != null ? ca.Cve_Colonia : null,
                    Street = ca != null ? ca.Street : null,
                    ExtNbr = ca != null ? ca.ExtNbr : null,
                    InnerNbr = ca != null ? ca.InnerNbr : null,

                    d_asenta = pc != null ? pc.d_asenta : null,
                    D_mnpio = pc != null ? pc.D_mnpio : null,
                    d_estado = pc != null ? pc.d_estado : null,
                    d_codigo = pc != null ? pc.d_codigo : null
                };

            var raw = await query.FirstOrDefaultAsync();

            if (raw == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Cliente no encontrado."
                };
                return response;
            }

            response.Result = new ClientDTO
            {
                ClientId = raw.ClientId,
                FullName = $"{raw.Name} {raw.LastName} {raw.SurName}".Replace("  ", " ").Trim(),
                Email = raw.Email,
                PhoneNumber = raw.Phone,
                Status = raw.Status,
                IdentificationNumber = raw.IdentificationNumber,

                Cve_CodigoPostal = raw.Cve_CodigoPostal ?? string.Empty,
                Cve_Estado = raw.Cve_Estado ?? string.Empty,
                Cve_Municipio = raw.Cve_Municipio ?? string.Empty,
                Cve_Colonia = raw.Cve_Colonia ?? string.Empty,
                Street = raw.Street ?? string.Empty,
                ExtNbr = raw.ExtNbr ?? string.Empty,
                InnerNbr = raw.InnerNbr ?? string.Empty,

                Address = raw.Street != null
                    ? $"{raw.Street} #{raw.ExtNbr}, {raw.d_asenta}, {raw.D_mnpio}, {raw.d_estado}, CP {raw.d_codigo}"
                    : null
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = userId,
                Module = "ZurichAPI-DataAccessClient",
                Action = "GetMyClientProfile",
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


    #region Métodos privados
    private static ReplyResponse SetError(ReplyResponse response, int code, string message)
    {
        response.Error = new ErrorDTO
        {
            Code = code,
            Message = message
        };

        return response;
    }

    private async Task<long> GenerateUniqueIdentificationNumberAsync()
    {
        for (int i = 0; i < 10; i++)
        {
            long candidate = Generate10DigitNumber();

            bool exists = await Context.TClients
                .AnyAsync(c => c.IdentificationNumber == candidate);

            if (!exists)
                return candidate;
        }

        throw new Exception("No fue posible generar un IdentificationNumber único.");
    }

    private static long Generate10DigitNumber()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        byte[] bytes = new byte[8];
        rng.GetBytes(bytes);

        ulong value = BitConverter.ToUInt64(bytes, 0);
        long number = (long)(value % 9000000000UL) + 1000000000L;

        return number;
    }

    #endregion
}
