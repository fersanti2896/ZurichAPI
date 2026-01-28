
using ZurichAPI.Models.Request.User;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.User;

namespace ZurichAPI.Data.SQL.Interfaces;

public interface IDataAccessUser
{
    Task<ReplyResponse> CreateUser(CreateUserRequest request);
    Task<LoginResponse> Login(LoginRequest request);
    Task<LoginResponse> RefreshToken(RefreshTokenRequest request);
}
