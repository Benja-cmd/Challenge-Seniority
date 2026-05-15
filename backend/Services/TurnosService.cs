using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.Helpers;
using TurnosMedicos.Models;
using TurnosMedicos.Services.Interfaces;

namespace TurnosMedicos.Services;

public class TurnosService : ITurnosService
{
    private readonly AppDbContext _context;

    public TurnosService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Turno>> GetAllAsync()
    {
        return await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .ToListAsync();
    }

    public async Task<Turno?> GetByIdAsync(int id)
    {
        return await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Turno> CrearTurnoAsync(Turno turno)
    {
        var paciente = await _context.Pacientes.FindAsync(turno.PacienteId);
        if (paciente == null)
            throw new KeyNotFoundException("Paciente no encontrado.");

        if (!paciente.IsActive)
            throw new InvalidOperationException("El paciente no se encuentra activo.");

        if (paciente.Bloqueado && paciente.FechaBloqueo.HasValue)
        {
            if (DateTime.UtcNow < paciente.FechaBloqueo.Value.AddDays(30))
                throw new InvalidOperationException("El paciente se encuentra bloqueado para agendar turnos online.");

            paciente.Bloqueado = false;
            paciente.FechaBloqueo = null;
            paciente.NoShowCount = 0;
        }

        var medicoExiste = await _context.Medicos.AnyAsync(m => m.Id == turno.MedicoId);
        if (!medicoExiste)
            throw new KeyNotFoundException("Médico no encontrado.");

        var turnoConflicto = await _context.Turnos.AnyAsync(t =>
            t.MedicoId == turno.MedicoId &&
            t.FechaHora == turno.FechaHora &&
            t.Estado != EstadoTurno.Cancelado);
        if (turnoConflicto)
            throw new InvalidOperationException("El médico ya tiene un turno en ese horario.");

        turno.FechaCreacion = DateTime.UtcNow;
        turno.Estado = EstadoTurno.Pendiente;
        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();
        return turno;
    }

    public async Task<Turno> CancelarTurnoAsync(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Turno no encontrado.");

        if (turno.Estado == EstadoTurno.Cancelado || turno.Estado == EstadoTurno.Atendido)
            throw new InvalidOperationException("El turno no puede cancelarse en su estado actual.");

        if (turno.FechaHora.EsCancelacionTardia())
        {
            turno.Estado = EstadoTurno.NoShow;
            turno.FechaCancelacion = DateTime.UtcNow;

            if (turno.Paciente != null)
            {
                turno.Paciente.NoShowCount++;
                if (turno.Paciente.NoShowCount >= 3)
                {
                    turno.Paciente.Bloqueado = true;
                    turno.Paciente.FechaBloqueo = DateTime.UtcNow;
                }
            }
        }
        else
        {
            turno.Estado = EstadoTurno.Cancelado;
            turno.FechaCancelacion = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return turno;
    }

    public async Task<Turno> MarcarAusenciaAsync(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Turno no encontrado.");

        if (turno.Estado != EstadoTurno.Pendiente && turno.Estado != EstadoTurno.Confirmado)
            throw new InvalidOperationException("Solo se puede marcar ausencia en turnos Pendientes o Confirmados.");

        if (!turno.FechaHora.IsPasado())
            throw new InvalidOperationException("No se puede marcar ausencia en un turno que aún no ocurrió.");

        turno.Estado = EstadoTurno.NoShow;

        if (turno.Paciente != null)
        {
            turno.Paciente.NoShowCount++;
            if (turno.Paciente.NoShowCount >= 3)
            {
                turno.Paciente.Bloqueado = true;
                turno.Paciente.FechaBloqueo = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return turno;
    }

    public async Task<Turno> ActualizarEstadoAsync(int id, EstadoTurno nuevoEstado)
    {
        var turno = await _context.Turnos.FindAsync(id)
            ?? throw new KeyNotFoundException("Turno no encontrado.");

        var transicionesValidas = new Dictionary<EstadoTurno, List<EstadoTurno>>
        {
            { EstadoTurno.Pendiente,  new List<EstadoTurno> { EstadoTurno.Confirmado, EstadoTurno.Cancelado } },
            { EstadoTurno.Confirmado, new List<EstadoTurno> { EstadoTurno.Atendido,   EstadoTurno.Cancelado } },
            { EstadoTurno.Cancelado,  new List<EstadoTurno>() },
            { EstadoTurno.Atendido,   new List<EstadoTurno>() },
            { EstadoTurno.NoShow,     new List<EstadoTurno>() },
        };

        if (!transicionesValidas[turno.Estado].Contains(nuevoEstado))
            throw new InvalidOperationException($"No se puede pasar de '{turno.Estado}' a '{nuevoEstado}'.");

        turno.Estado = nuevoEstado;
        await _context.SaveChangesAsync();
        return turno;
    }
}
