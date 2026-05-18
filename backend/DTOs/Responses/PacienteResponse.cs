namespace TurnosMedicos.DTOs.Responses;

public class PacienteResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public bool Bloqueado { get; set; }
    public int NoShowCount { get; set; }
    public DateTime CreatedAt { get; set; }
}