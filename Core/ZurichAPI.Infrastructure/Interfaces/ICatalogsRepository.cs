
using ZurichAPI.Models.Request.Catalogs;
using ZurichAPI.Models.Response.Catalogs;

namespace ZurichAPI.Infrastructure.Interfaces;

public interface ICatalogsRepository
{
    Task<GetStatesResponse> GetStates(int IdUser);
    Task<MunicipalityByStateResponse> GetMunicipalityByState(MunicipalityByStateRequest request, int IdUser);
    Task<TownByStateAndMunicipalityResponse> GetTownByStateAndMunicipality(TownByStateAndMunicipalityRequest request, int IdUser);
    Task<CPResponse> GetCP(CPRequest request, int IdUser);
    Task<PolicyTypesResponse> GetPolicyTypes(int IdUser);
    Task<PolicyStatusResponse> GetPolicyStatus(int IdUser);
}
