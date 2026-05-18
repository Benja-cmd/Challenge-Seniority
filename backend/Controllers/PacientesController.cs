using Microsoft.AspNetCore.Mvc;
using TurnosMedicos.DTOs.Requests;
using TurnosMedicos.Services.Interfaces;

namespace TurnosMedicos.Controllers;

[ApiController]
[Route("[controller]")]
public class PacientesController : ControllerBase
{
    private readonly IPacientesService _pacientesService;

    public PacientesController(IPacientesService pacientesService)
    {
        _pacientesService = pacientesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var pacientes = await _pacientesService.GetAllAsync();
        return Ok(pacientes);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var paciente = await _pacientesService.GetByIdAsync(id);
        if (paciente == null) return NotFound();
        return Ok(paciente);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CrearPacienteRequest request)
    {
        try
        {
            var created = await _pacientesService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ActualizarPacienteRequest request)
    {
        try
        {
            var updated = await _pacientesService.UpdateAsync(id, request);
            return Ok(updated);
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _pacientesService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensaje = ex.Message });
        }
    }
}