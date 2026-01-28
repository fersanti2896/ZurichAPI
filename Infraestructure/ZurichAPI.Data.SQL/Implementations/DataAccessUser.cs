
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ZurichAPI.Data.SQL.Entities;
using ZurichAPI.Data.SQL.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Helpers;
using ZurichAPI.Models.Request.User;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.User;

namespace ZurichAPI.Data.SQL.Implementations;

public class DataAccessUser : IDataAccessUser
{
    private IDataAccessLogs IDataAccessLogs;
    private readonly IConfiguration _configuration;
    public AppDbContext Context { get; set; }
    private static readonly TimeZoneInfo _cdmxZone = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
    private static DateTime NowCDMX => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _cdmxZone);

    public DataAccessUser(AppDbContext appDbContext, IDataAccessLogs iDataAccessLogs, IConfiguration configurations)
    {
        Context = appDbContext;
        IDataAccessLogs = iDataAccessLogs;
        _configuration = configurations;
    }

    public async Task<ReplyResponse> CreateUser(CreateUserRequest request)
    {
        ReplyResponse response = new();

        try
        {
            var emailNormalized = request.Email.Trim().ToUpper();

            var roleExists = await Context.TRol.AnyAsync(r => r.RoleId == request.RoleId && r.Status == 1);
            if (!roleExists)
                return SetError(response, 400, "RoleId inválido.");

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
                MLastName = request.MLastName?.Trim(),
                Email = request.Email.Trim(),
                PasswordHash = encryptedPassword,
                RoleId = request.RoleId,
                Status = 1,
                CreateDate = NowCDMX,
                CreateUser = 1
            };

            await Context.TUsers.AddAsync(user);
            await Context.SaveChangesAsync();

            await tx.CommitAsync();

            response.Result = new ReplyDTO
            {
                Status = true,
                Msg = "Usuario creado"
            };

            return response;
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                IdUser = 1,
                Module = "ZurichAPI-DataAccessUser",
                Action = "CreateUser",
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

    public async Task<LoginResponse> Login(LoginRequest request)
    {
        LoginResponse response = new();

        try
        {
            string passwordNormalized = request.Password.ToUpper();
            string encryptedPassword = EncryptDecrypt.EncryptString(passwordNormalized);

            var user = await Context.TUsers.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == encryptedPassword && u.Status == 1);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario o contraseña inválidos."
                };
                return response;
            }

            var accessToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();
            var expiration = NowCDMX.AddDays(1);

            var newToken = new TRefreshTokens
            {
                UserId = user.UserId,
                Token = refreshToken,
                ExpirationDate = expiration,
                IsRevoked = false
            };

            await Context.TRefreshTokens.AddAsync(newToken);
            await Context.SaveChangesAsync();


            response.Result = new LoginDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshToken,
                FullName = $"{user.FirstName} {user.LastName} {user.MLastName ?? string.Empty}",
                RoleId = user.RoleId,
                RoleDescription = user.Role?.Name
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "ZurichAPI-DataAccessUser",
                Action = "Login",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Error en el proceso de autenticación."
            };
        }

        return response;
    }

    public async Task<LoginResponse> RefreshToken(RefreshTokenRequest request)
    {
        LoginResponse response = new();

        try
        {
            // Buscar refresh token válido en la BD
            var storedToken = await Context.TRefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken && !t.IsRevoked && t.ExpirationDate > NowCDMX);

            if (storedToken is null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 401,
                    Message = "Refresh token inválido o expirado."
                };

                return response;
            }

            // Obtener usuario asociado
            var user = await Context.TUsers.Include(u => u.Role)
                                           .FirstOrDefaultAsync(u => u.UserId == storedToken.UserId && u.Status == 1);

            if (user == null)
            {
                response.Error = new ErrorDTO
                {
                    Code = 404,
                    Message = "Usuario no encontrado o inactivo."
                };

                return response;
            }

            // Generar nuevos tokens
            var newAccessToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();
            var newExpiration = NowCDMX.AddDays(2);

            await Context.TRefreshTokens.AddAsync(new TRefreshTokens
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpirationDate = newExpiration
            });

            await Context.SaveChangesAsync();

            // Devolver nuevos tokens
            response.Result = new LoginDTO
            {
                UserId = user.UserId,
                Email = user.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                FullName = $"{user.FirstName} {user.LastName} {user.MLastName ?? string.Empty}",
                RoleId = user.RoleId,
                RoleDescription = user.Role?.Name
            };
        }
        catch (Exception ex)
        {
            await IDataAccessLogs.Create(new LogsDTO
            {
                Module = "ZurichAPI-DataAccessUser",
                Action = "RefreshToken",
                Message = $"Exception: {ex.Message}",
                InnerException = $"Inner: {ex.InnerException?.Message}"
            });

            response.Error = new ErrorDTO
            {
                Code = 500,
                Message = "Error al procesar el refresh token."
            };
        }

        return response;
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

    private string GenerateJwtToken(TUsers user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", user.UserId.ToString()),
            new Claim("Role", user.RoleId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: NowCDMX.AddMinutes(Convert.ToInt32(jwtSettings["ExpiryMinutes"])),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        return Convert.ToBase64String(randomNumber);
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
}
