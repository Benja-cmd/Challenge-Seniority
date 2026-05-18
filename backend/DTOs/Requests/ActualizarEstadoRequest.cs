using TurnosMedicos.Models;

namespace TurnosMedicos.DTOs.Requests;

public class ActualizarEstadoRequest
{
    public EstadoTurno Estado { get; set; }
}
