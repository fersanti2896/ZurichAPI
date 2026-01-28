
using System.ComponentModel.DataAnnotations;
using ZurichAPI.Models.Request.Clients;

namespace ZurichAPI.Models.Request.User;

public class CreateUserRequest
{
    [Required, MaxLength(80)]
    public string FirstName { get; set; }

    [Required, MaxLength(80)]
    public string LastName { get; set; }

    [MaxLength(80)]
    public string? MLastName { get; set; }

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } 

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int RoleId { get; set; }
}
