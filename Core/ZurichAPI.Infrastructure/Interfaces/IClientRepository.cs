using ZurichAPI.Models.Request.Clients;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Clients;

namespace ZurichAPI.Infrastructure.Interfaces;

public interface IClientRepository
{
    Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId);
    Task<ReplyResponse> UpdateMyProfile(UpdateMyProfileRequest request, int userId);
    Task<ReplyResponse> UpdateClientByAdmin(UpdateClientRequest request, int userId);
    Task<ClientsResponse> GetAllClients(GetClientsRequest request, int userId);
    Task<ReplyResponse> DeleteClient(DeleteClienteRequest request, int userId);
    Task<ClientResponse> GetMyClientProfile(int userId);
}
