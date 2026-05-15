using Microsoft.AspNetCore.Mvc;
using TurnosMedicos.Models;
using TurnosMedicos.Services.Interfaces;

namespace TurnosMedicos.Controllers;

[ApiController]
[Route("[controller]")]
public class TurnosController : ControllerBase
{
    private readonly ITurnosService _turnosService;

    public TurnosController(ITurnosService turnosService)
    {
        _turnosService = turnosService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var turnos = await _turnosService.GetAllAsync();
        return Ok(turnos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var turno = await _turnosService.GetByIdAsync(id);
        if (turno == null) return NotFound();
        return Ok(turno);
    }

    [HttpPost]
    public async Task<IActionResult> CrearTurno([FromBody] Turno turno)
    {
        try
        {
            var created = await _turnosService.CrearTurnoAsync(turno);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> CancelarTurno(int id)
    {
        try
        {
            var turno = await _turnosService.CancelarTurnoAsync(id);
            return Ok(turno);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPost("{id}/ausencia")]
    public async Task<IActionResult> MarcarAusencia(int id)
    {
        try
        {
            var turno = await _turnosService.MarcarAusenciaAsync(id);
            return Ok(turno);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> ActualizarEstado(int id, [FromBody] ActualizarEstadoRequest request)
    {
        try
        {
            var turno = await _turnosService.ActualizarEstadoAsync(id, request.Estado);
            return Ok(turno);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}

public class ActualizarEstadoRequest
{
    public EstadoTurno Estado { get; set; }
}