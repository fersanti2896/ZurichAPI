
using System.ComponentModel.DataAnnotations;

namespace ZurichAPI.Models.Request.Clients;

public class UpdateMyProfileRequest
{
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
