
namespace ZurichAPI.Models.DTOs;

public class ClientDTO
{
    public int ClientId { get; set; }
    public string FullName { get; set; } = null!;       // FullName
    public string? Address { get; set; }                   // Dirección
    public string? PhoneNumber { get; set; }               // Teléfono de contacto
    public string? Email { get; set; }                     // Correo electrónico
    public string Cve_CodigoPostal { get; set; }           // Clave de CP
    public string Cve_Estado { get; set; }                 // Clave del Estado
    public string Cve_Municipio { get; set; }              // Clave de Municipio
    public string Cve_Colonia { get; set; }                // Clave de la Colonia
    public string Street { get; set; }                     // Calle
    public string ExtNbr { get; set; }                     // Número Ext
    public string InnerNbr { get; set; }                   // Número Int
    public int Status { get; set; }
}
