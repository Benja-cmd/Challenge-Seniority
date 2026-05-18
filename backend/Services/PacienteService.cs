using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.DTOs.Requests;
using TurnosMedicos.DTOs.Responses;
using TurnosMedicos.Services.Interfaces;

namespace TurnosMedicos.Services;

public class PacientesService : IPacientesService
{
    private readonly AppDbContext _context;

    public PacientesService(AppDbContext context)
    {
        _context = context;
    }

    private static PacienteResponse MapToResponse(Models.Paciente p) => new()
    {
        Id = p.Id,
        NombreCompleto = p.NombreCompleto,
        DNI = p.DNI,
        Email = p.Email,
        Telefono = p.Telefono,
        Bloqueado = p.Bloqueado,
        NoShowCount = p.NoShowCount,
        CreatedAt = p.CreatedAt,
    };

    public async Task<List<PacienteResponse>> GetAllAsync()
    {
        var pacientes = await _context.Pacientes
            .Where(p => p.IsActive)
            .ToListAsync();
        return pacientes.Select(MapToResponse).ToList();
    }

    public async Task<PacienteResponse?> GetByIdAsync(int id)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        return paciente == null ? null : MapToResponse(paciente);
    }

    public async Task<PacienteResponse> CreateAsync(CrearPacienteRequest request)
    {
        var dniExiste = await _context.Pacientes
            .AnyAsync(p => p.DNI == request.DNI && p.IsActive);
        if (dniExiste)
            throw new InvalidOperationException("Ya existe un paciente con ese DNI.");

        var paciente = new Models.Paciente
        {
            NombreCompleto = request.NombreCompleto,
            DNI = request.DNI,
            Email = request.Email,
            Telefono = request.Telefono,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            NoShowCount = 0,
            Bloqueado = false,
            FechaBloqueo = null,
        };

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return MapToResponse(paciente);
    }

    public async Task<PacienteResponse> UpdateAsync(int id, ActualizarPacienteRequest request)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var dniExiste = await _context.Pacientes
            .AnyAsync(p => p.DNI == request.DNI && p.Id != id && p.IsActive);
        if (dniExiste)
            throw new InvalidOperationException("Ya existe otro paciente con ese DNI.");

        paciente.NombreCompleto = request.NombreCompleto;
        paciente.DNI = request.DNI;
        paciente.Email = request.Email;
        paciente.Telefono = request.Telefono;

        await _context.SaveChangesAsync();
        return MapToResponse(paciente);
    }

    public async Task DeleteAsync(int id)
    {
        var paciente = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        paciente.IsActive = false;
        await _context.SaveChangesAsync();
    }
}