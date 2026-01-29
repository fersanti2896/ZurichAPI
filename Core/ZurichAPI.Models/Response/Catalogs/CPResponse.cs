using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Catalogs;

public class CPResponse : BaseResponse
{
    public CPDTO? Result { get; set; }
}
