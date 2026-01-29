
namespace ZurichAPI.Models.Request.Policys;

public class GetPolicysRequest
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? PolicyTypeId { get; set; }
    public int? PolicyStatusId { get; set; }
}
