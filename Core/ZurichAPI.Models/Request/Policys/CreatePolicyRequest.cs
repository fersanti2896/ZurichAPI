using System.ComponentModel.DataAnnotations;

namespace ZurichAPI.Models.Request.Policys;

public class CreatePolicyRequest
{
    [Required(ErrorMessage = "El cliente es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El cliente no es válido.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "El tipo de póliza es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El tipo de póliza no es válido.")]
    public int PolicyTypeId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de expiración es obligatoria.")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "El monto asegurado es obligatorio.")]
    [Range(typeof(decimal), "0.01", "999999999999", ErrorMessage = "El monto asegurado debe ser un valor positivo.")]
    public decimal InsuredAmount { get; set; }
}
