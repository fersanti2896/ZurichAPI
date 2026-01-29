namespace ZurichAPI.Models.DTOs;

public class PolicyDTO
{
    public int PolicyId { get; set; }
    public int ClientId { get; set; }
    public string FullName { get; set; }
    public int PolicyTypeId { get; set; }
    public string PolicyName { get; set; }
    public int PolicyStatusId { get; set; }
    public string StatusName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal InsuredAmount { get; set; }
}
