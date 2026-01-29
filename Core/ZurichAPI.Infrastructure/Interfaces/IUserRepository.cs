
using ZurichAPI.Models.Request.Clients;
using ZurichAPI.Models.Request.User;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.User;

namespace ZurichAPI.Infrastructure.Interfaces;

public interface IUserRepository
{
    Task<ReplyResponse> CreateUser(CreateUserRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
}
