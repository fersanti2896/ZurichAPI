using System.ComponentModel.DataAnnotations;

namespace ZurichAPI.Models.Request.Clients;

public class UpdateClientRequest
{
    private const string NameRegex = @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ ]+$";

    [Required(ErrorMessage = "ClientId es requerido.")]
    [Range(1, int.MaxValue, ErrorMessage = "ClientId inválido.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [MaxLength(30, ErrorMessage = "El nombre no puede exceder 30 caracteres.")]
    [RegularExpression(NameRegex, ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string FirstName { get; set; } = null!;

    [Required(ErrorMessage = "El apellido paterno es requerido.")]
    [MaxLength(30, ErrorMessage = "El apellido paterno no puede exceder 30 caracteres.")]
    [RegularExpression(NameRegex, ErrorMessage = "El apellido paterno solo puede contener letras y espacios.")]
    public string LastName { get; set; } = null!;

    [MaxLength(30, ErrorMessage = "El apellido materno no puede exceder 30 caracteres.")]
    [RegularExpression(NameRegex, ErrorMessage = "El apellido materno solo puede contener letras y espacios.")]
    public string? MLastName { get; set; }

    [RegularExpression(@"^$|^.{6,100}$", ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres.")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "El teléfono es requerido.")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "El teléfono debe contener exactamente 10 dígitos.")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "El código postal es requerido.")]
    public string Cve_CodigoPostal { get; set; } = null!;

    [Required(ErrorMessage = "El estado es requerido.")]
    public string Cve_Estado { get; set; } = null!;

    [Required(ErrorMessage = "El municipio es requerido.")]
    public string Cve_Municipio { get; set; } = null!;

    [Required(ErrorMessage = "La colonia es requerida.")]
    public string Cve_Colonia { get; set; } = null!;

    [Required(ErrorMessage = "La calle es requerida.")]
    public string Street { get; set; } = null!;

    [Required(ErrorMessage = "El número exterior es requerido.")]
    public string ExtNbr { get; set; } = null!;

    public string? InnerNbr { get; set; }
}
