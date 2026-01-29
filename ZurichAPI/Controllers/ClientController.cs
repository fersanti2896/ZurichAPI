using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Clients;
using ZurichAPI.Models.Response;

namespace ZurichAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientRepository IClientRepository;

    public ClientController(IClientRepository iClientRepository)
    {
        IClientRepository = iClientRepository;
    }

    /// <summary>
    /// Listado de clientes del sistema
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "clients.manage")]
    [HttpPost]
    [Route("AllClients")]
    public async Task<IActionResult> GetAllClients(GetClientsRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.GetAllClients(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene información del cliente
    /// </summary>
    [Authorize]
    [HttpGet]
    [Route("MyClientProfile")]
    public async Task<IActionResult> MyClientProfile()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.GetMyClientProfile(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }


    /// <summary>
    /// Crea un cliente
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "clients.manage")]
    [HttpPost]
    [Route("CreateClient")]
    public async Task<IActionResult> CreateClient(CreateClientRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildModelStateError());

        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await IClientRepository.CreateClient(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualiza información de un cliente por si mismo
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "profile.self.edit")]
    [HttpPost]
    [Route("UpdateMyProfile")]
    public async Task<IActionResult> UpdateMyProfile(UpdateMyProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildModelStateError());

        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var result = await IClientRepository.UpdateMyProfile(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Actualiza informacion de un cliente por Administración
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>

    [Authorize(Policy = "clients.manage")]
    [HttpPost]
    [Route("UpdateClient")]
    public async Task<IActionResult> UpdateClient(UpdateClientRequest request)
    {
        if (request.Password != null && string.IsNullOrWhiteSpace(request.Password))
            request.Password = null;

        if (!ModelState.IsValid)
            return BadRequest(BuildModelStateError());

        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.UpdateClientByAdmin(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Elimina un cliente siempre y cuando no tenga pólizas activadas
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    [Authorize(Policy = "clients.manage")]
    [HttpDelete]
    [Route("DeleteClient")]
    public async Task<IActionResult> DeleteClient(DeleteClienteRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IClientRepository.DeleteClient(request, userId);

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
