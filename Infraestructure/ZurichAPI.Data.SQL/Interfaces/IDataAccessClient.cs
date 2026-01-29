
using ZurichAPI.Models.Request.Clients;
using ZurichAPI.Models.Response;
using ZurichAPI.Models.Response.Clients;

namespace ZurichAPI.Data.SQL.Interfaces;

public interface IDataAccessClient
{
    Task<ReplyResponse> CreateClient(CreateClientRequest request, int userId);
    Task<ReplyResponse> UpdateMyProfile(UpdateMyProfileRequest request, int userId);
    Task<ReplyResponse> UpdateClientByAdmin(UpdateClientRequest request, int userId);
    Task<ClientsResponse> GetAllClients(int userId);
}
