using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Catalogs;

public class MunicipalityByStateResponse : BaseResponse
{
    public List<MunicipalityDTO>? Result { get; set; }
}
