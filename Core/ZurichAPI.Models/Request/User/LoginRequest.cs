
using System.ComponentModel.DataAnnotations;

namespace ZurichAPI.Models.Request.User;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
