using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Policys;
using ZurichAPI.Models.Response;

namespace ZurichAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PolicyController : ControllerBase
{
    private readonly IPolicyRepository IPolicyRepository;

    public PolicyController(IPolicyRepository iPoliciyRepository)
    {
        IPolicyRepository = iPoliciyRepository;
    }

    /// <summary>
    /// Crea una póliza para un usuario
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("CreatePolicy")]
    [Authorize(Policy = "policies.manage")]
    public async Task<IActionResult> CreatePolicy(CreatePolicyRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(BuildModelStateError());

        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IPolicyRepository.CreatePolicy(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene todas las pólizas (filtro por tipo de póliza, estatus de la póliza o por rango de fechas
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "policies.manage")]
    [HttpPost]
    [Route("AllPolicys")]
    public async Task<IActionResult> AllPolicys(GetPolicysRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IPolicyRepository.GetAllPolicys(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Polizas de un cliente
    /// </summary>
    /// <returns></returns>
    [Authorize(Policy = "policies.self.view")]
    [HttpGet]
    [Route("MyPolicys")]
    public async Task<IActionResult> MyPolicys()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IPolicyRepository.GetMyPolicys(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Cliente solicita la cancelación de una póliza
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "policies.self.cancel")]
    [HttpPost]
    [Route("RequestCancelPolicy")]
    public async Task<IActionResult> RequestCancelPolicy(CancelPolicyRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IPolicyRepository.RequestCancelPolicy(request, userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Se aprueba la cancelación de una póliza
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize(Policy = "policies.manage")]
    [HttpPost]
    [Route("ApproveCancelPolicy")]
    public async Task<IActionResult> ApproveCancelPolicy(CancelPolicyRequest request)
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await IPolicyRepository.ApproveCancelPolicy(request, userId);

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
