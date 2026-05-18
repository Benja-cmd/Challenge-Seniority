namespace TurnosMedicos.DTOs.Requests;

public class CrearPacienteRequest
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}