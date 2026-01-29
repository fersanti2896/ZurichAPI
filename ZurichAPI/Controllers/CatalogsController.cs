using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZurichAPI.Infrastructure.Interfaces;
using ZurichAPI.Models.DTOs;
using ZurichAPI.Models.Request.Catalogs;
using ZurichAPI.Models.Response.Catalogs;
using ZurichAPI.Models.Response;

namespace ZurichAPI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CatalogsController : ControllerBase
{
    private readonly ICatalogsRepository ICatalogsRepository;

    public CatalogsController(ICatalogsRepository iCatalogsRepository)
    {
        ICatalogsRepository = iCatalogsRepository;
    }

    /// <summary>
    /// Obtiene los estados
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetStates")]
    public async Task<IActionResult> GetStates()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICatalogsRepository.GetStates(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Obtienes los municipios por estado
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetMunicipalityByState")]
    public async Task<IActionResult> GetMunicipalityByState(MunicipalityByStateRequest request)
    {
        return await HandlePostRequest<MunicipalityByStateRequest, MunicipalityByStateResponse>(request, ICatalogsRepository.GetMunicipalityByState);
    }

    /// <summary>
    /// Obtiene las colonias por municipio y estado
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetTownByStateAndMunicipality")]
    public async Task<IActionResult> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request)
    {
        return await HandlePostRequest<TownByStateAndMunicipalityRequest, TownByStateAndMunicipalityResponse>(request, ICatalogsRepository.GetTownByStateAndMunicipality);
    }

    /// <summary>
    /// Obtiene información de un CP
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("GetCP")]
    public async Task<IActionResult> GetCP(CPRequest request)
    {
        return await HandlePostRequest<CPRequest, CPResponse>(request, ICatalogsRepository.GetCP);
    }

    /// <summary>
    /// Obtiene los tipos de pólizas
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetPolicyTypes")]
    public async Task<IActionResult> GetPolicyTypes()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICatalogsRepository.GetPolicyTypes(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Obtiene los estatus de pólizas
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetPolicyStatus")]
    public async Task<IActionResult> GetPolicyStatus()
    {
        var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

        var result = await ICatalogsRepository.GetPolicyStatus(userId);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }

    #region
    private async Task<IActionResult> HandlePostRequest<TRequest, TResponse>(TRequest request, Func<TRequest, int, Task<TResponse>> businessLogicMethod)
    where TResponse : BaseResponse
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            var errorMessage = string.Join(" ", errors);

            return BadRequest(new ErrorDTO
            {
                Code = 400,
                Message = errorMessage
            });
        }

        int IdUser = Convert.ToInt32(User.Claims.Where(x => x.Type == "UserId").First().Value);
        var result = await businessLogicMethod(request, IdUser);

        if (result.Error != null)
            return BadRequest(result);

        return Ok(result);
    }
    #endregion
}
