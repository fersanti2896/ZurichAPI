using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZurichAPI.Infrastructure.Interfaces;

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
}
