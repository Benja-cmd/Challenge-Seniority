using Microsoft.AspNetCore.Mvc;
using TurnosMedicos.Models;
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
    public async Task<IActionResult> Create([FromBody] Paciente paciente)
    {
        try
        {
            var created = await _pacientesService.CreateAsync(paciente);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Paciente paciente)
    {
        try
        {
            var updated = await _pacientesService.UpdateAsync(id, paciente);
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