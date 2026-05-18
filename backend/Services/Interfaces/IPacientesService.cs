using TurnosMedicos.DTOs.Requests;
using TurnosMedicos.DTOs.Responses;

namespace TurnosMedicos.Services.Interfaces;

public interface IPacientesService
{
    Task<List<PacienteResponse>> GetAllAsync();
    Task<PacienteResponse?> GetByIdAsync(int id);
    Task<PacienteResponse> CreateAsync(CrearPacienteRequest request);
    Task<PacienteResponse> UpdateAsync(int id, ActualizarPacienteRequest request);
    Task DeleteAsync(int id);
}