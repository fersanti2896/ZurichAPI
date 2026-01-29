using ZurichAPI.Models.DTOs;

namespace ZurichAPI.Models.Response.Catalogs;

public class TownByStateAndMunicipalityResponse : BaseResponse
{
    public List<TownDTO>? Result { get; set; }
}