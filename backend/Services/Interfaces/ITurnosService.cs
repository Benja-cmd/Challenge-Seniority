using TurnosMedicos.DTOs.Requests;
using TurnosMedicos.DTOs.Responses;

namespace TurnosMedicos.Services.Interfaces;

public interface ITurnosService
{
    Task<List<TurnoResponse>> GetAllAsync();
    Task<TurnoResponse?> GetByIdAsync(int id);
    Task<TurnoResponse> CrearTurnoAsync(CrearTurnoRequest request);
    Task<TurnoResponse> CancelarTurnoAsync(int id);
    Task<TurnoResponse> MarcarAusenciaAsync(int id);
    Task<TurnoResponse> ActualizarEstadoAsync(int id, ActualizarEstadoRequest request);
}