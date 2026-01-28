
using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.User;

public class LoginResponse : BaseResponse
{
    public LoginDTO? Result { get; set; }
}
