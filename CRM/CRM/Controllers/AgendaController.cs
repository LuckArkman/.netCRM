using DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using DTOs;
using Interfaces;

namespace GabineteDigital.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AgendaController : ControllerBase
    {
        private readonly IAgendaService _agendaService;

        public AgendaController(IAgendaService agendaService)
        {
            _agendaService = agendaService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedAccessException("ID do usuário não encontrado no token.");
            }
            return Guid.Parse(userIdString);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretario,Assessor")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompromissoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCompromisso([FromBody] CreateCompromissoDto dto)
        {
            var compromisso = await _agendaService.CreateCompromissoAsync(dto, GetCurrentUserId());
            if (compromisso == null) return BadRequest("Não foi possível criar o compromisso.");
            return CreatedAtAction(nameof(GetCompromisso), new { id = compromisso.Id }, compromisso);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompromissoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCompromisso(Guid id)
        {
            var compromisso = await _agendaService.GetCompromissoByIdAsync(id);
            if (compromisso == null) return NotFound();
            return Ok(compromisso);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompromissoDto>))]
        public async Task<IActionResult> GetCompromissos(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] Guid? responsavelId = null)
        {
            // Se nenhum responsável for especificado e o usuário não for Admin, filtra pelos compromissos do próprio usuário
            if (!responsavelId.HasValue && !User.IsInRole("Admin") && !User.IsInRole("Secretario"))
            {
                responsavelId = GetCurrentUserId();
            }

            var compromissos = await _agendaService.GetCompromissosByDateRangeAsync(startDate, endDate, responsavelId);
            return Ok(compromissos);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretario,Assessor")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCompromisso(Guid id, [FromBody] UpdateCompromissoDto dto)
        {
            var updated = await _agendaService.UpdateCompromissoAsync(id, dto, GetCurrentUserId());
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Secretario")] // Apenas Admin e Secretário podem deletar
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCompromisso(Guid id)
        {
            var deleted = await _agendaService.DeleteCompromissoAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}