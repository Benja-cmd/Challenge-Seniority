using TurnosMedicos.Models;
namespace TurnosMedicos.DTOs.Responses;

public class TurnoResponse
{
    public int Id { get; set; }
    public int? PacienteId { get; set; }
    public string PacienteNombre { get; set; } = string.Empty;
    public int MedicoId { get; set; }
    public string MedicoNombre { get; set; } = string.Empty;
    public string MedicoEspecialidad { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }
    public EstadoTurno Estado { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaCancelacion { get; set; }
    public string Motivo { get; set; } = string.Empty;
}