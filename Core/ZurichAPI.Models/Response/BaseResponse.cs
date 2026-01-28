using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response;

public class BaseResponse
{
    public ErrorDTO? Error { get; set; }
}
