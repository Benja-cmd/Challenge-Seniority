using Microsoft.EntityFrameworkCore;
using TurnosMedicos.Data;
using TurnosMedicos.DTOs.Requests;
using TurnosMedicos.DTOs.Responses;
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

    private static TurnoResponse MapToResponse(Turno t) => new()
    {
        Id = t.Id,
        PacienteId = t.PacienteId,
        PacienteNombre = t.Paciente?.NombreCompleto ?? string.Empty,
        MedicoId = t.MedicoId,
        MedicoNombre = t.Medico?.NombreCompleto ?? string.Empty,
        MedicoEspecialidad = t.Medico?.Especialidad ?? string.Empty,
        FechaHora = t.FechaHora,
        Estado = t.Estado,
        FechaCreacion = t.FechaCreacion,
        FechaCancelacion = t.FechaCancelacion,
        Motivo = t.Motivo,
    };

    public async Task<List<TurnoResponse>> GetAllAsync()
    {
        var turnos = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .ToListAsync();
        return turnos.Select(MapToResponse).ToList();
    }

    public async Task<TurnoResponse?> GetByIdAsync(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .FirstOrDefaultAsync(t => t.Id == id);
        return turno == null ? null : MapToResponse(turno);
    }

    public async Task<TurnoResponse> CrearTurnoAsync(CrearTurnoRequest request)
    {
        var paciente = await _context.Pacientes.FindAsync(request.PacienteId)
            ?? throw new KeyNotFoundException("Paciente no encontrado.");

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

        var medico = await _context.Medicos.FindAsync(request.MedicoId)
            ?? throw new KeyNotFoundException("Médico no encontrado.");

        var turnoConflicto = await _context.Turnos.AnyAsync(t =>
            t.MedicoId == request.MedicoId &&
            t.FechaHora == request.FechaHora &&
            t.Estado != EstadoTurno.Cancelado);
        if (turnoConflicto)
            throw new InvalidOperationException("El médico ya tiene un turno en ese horario.");

        var turno = new Turno
        {
            PacienteId = request.PacienteId,
            MedicoId = request.MedicoId,
            FechaHora = request.FechaHora,
            Motivo = request.Motivo,
            FechaCreacion = DateTime.UtcNow,
            Estado = EstadoTurno.Pendiente,
        };

        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();

        turno.Paciente = paciente;
        turno.Medico = medico;

        return MapToResponse(turno);
    }

    public async Task<TurnoResponse> CancelarTurnoAsync(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
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
        return MapToResponse(turno);
    }

    public async Task<TurnoResponse> MarcarAusenciaAsync(int id)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
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
        return MapToResponse(turno);
    }

    public async Task<TurnoResponse> ActualizarEstadoAsync(int id, ActualizarEstadoRequest request)
    {
        var turno = await _context.Turnos
            .Include(t => t.Paciente)
            .Include(t => t.Medico)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Turno no encontrado.");

        var transicionesValidas = new Dictionary<EstadoTurno, List<EstadoTurno>>
        {
            { EstadoTurno.Pendiente,  new List<EstadoTurno> { EstadoTurno.Confirmado, EstadoTurno.Cancelado } },
            { EstadoTurno.Confirmado, new List<EstadoTurno> { EstadoTurno.Atendido,   EstadoTurno.Cancelado } },
            { EstadoTurno.Cancelado,  new List<EstadoTurno>() },
            { EstadoTurno.Atendido,   new List<EstadoTurno>() },
            { EstadoTurno.NoShow,     new List<EstadoTurno>() },
        };

        if (!transicionesValidas[turno.Estado].Contains(request.Estado))
            throw new InvalidOperationException($"No se puede pasar de '{turno.Estado}' a '{request.Estado}'.");

        turno.Estado = request.Estado;
        await _context.SaveChangesAsync();
        return MapToResponse(turno);
    }
}