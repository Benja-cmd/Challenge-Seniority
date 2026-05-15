using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.Models;
using TurnosMedicos.Services.Interfaces;

namespace TurnosMedicos.Services;

public class PacientesService : IPacientesService
{
    private readonly AppDbContext _context;

    public PacientesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Paciente>> GetAllAsync()
    {
        return await _context.Pacientes
            .Where(p => p.IsActive)
            .ToListAsync();
    }

    public async Task<Paciente?> GetByIdAsync(int id)
    {
        return await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
    }

    public async Task<Paciente> CreateAsync(Paciente paciente)
    {
        var dniExiste = await _context.Pacientes
            .AnyAsync(p => p.DNI == paciente.DNI && p.IsActive);
        if (dniExiste)
            throw new InvalidOperationException("Ya existe un paciente con ese DNI.");

        paciente.CreatedAt = DateTime.UtcNow;
        paciente.IsActive = true;
        paciente.NoShowCount = 0;
        paciente.Bloqueado = false;
        paciente.FechaBloqueo = null;

        _context.Pacientes.Add(paciente);
        await _context.SaveChangesAsync();
        return paciente;
    }

    public async Task<Paciente> UpdateAsync(int id, Paciente paciente)
    {
        var existing = await _context.Pacientes
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

        var dniExiste = await _context.Pacientes
            .AnyAsync(p => p.DNI == paciente.DNI && p.Id != id && p.IsActive);
        if (dniExiste)
            throw new InvalidOperationException("Ya existe otro paciente con ese DNI.");

        existing.NombreCompleto = paciente.NombreCompleto;
        existing.DNI = paciente.DNI;
        existing.Email = paciente.Email;
        existing.Telefono = paciente.Telefono;

        await _context.SaveChangesAsync();
        return existing;
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