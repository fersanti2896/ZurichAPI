using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.User;
using ZurichAPI.Models.Response;

namespace ZurichAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly IUserRepository IUserRepository;

    public UserController(IUserRepository iUserRepository)
    {
        IUserRepository = iUserRepository;
    }

    /// <summary>
    /// Servicio que crea un usuario con rol admin
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("CreateUser")]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildModelStateError());

        var result = await IUserRepository.CreateUser(request);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Servicio de logueo de usuario
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildModelStateError());

        var result = await IUserRepository.Login(request);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Servicio de refrescar el token
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequest request)
    {
        var result = await IUserRepository.RefreshToken(request);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    private ReplyResponse BuildModelStateError()
    {
        return new ReplyResponse
        {
            Error = new ErrorDTO
            {
                Code = 400,
                Message = string.Join(" | ",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
            }
        };
    }
}
