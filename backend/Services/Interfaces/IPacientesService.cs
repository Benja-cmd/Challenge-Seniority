using TurnosMedicos.Models;

namespace TurnosMedicos.Services.Interfaces;

public interface IPacientesService
{
    Task<List<Paciente>> GetAllAsync();
    Task<Paciente?> GetByIdAsync(int id);
    Task<Paciente> CreateAsync(Paciente paciente);
    Task<Paciente> UpdateAsync(int id, Paciente paciente);
    Task DeleteAsync(int id);
}