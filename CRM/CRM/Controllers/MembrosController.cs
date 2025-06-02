using DTOs;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize(Roles = "Admin")] // Apenas administradores podem gerenciar membros
    [ApiController]
    [Route("api/[controller]")]
    public class MembrosController : ControllerBase
    {
        private readonly IUserService _userService;

        public MembrosController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<MembroEquipeDto>))]
        public async Task<IActionResult> GetMembros()
        {
            var membros = await _userService.GetAllMembrosEquipeAsync();
            return Ok(membros);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MembroEquipeDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMembro(Guid id)
        {
            var membro = await _userService.GetMembroEquipeByIdAsync(id);
            if (membro == null) return NotFound();
            return Ok(membro);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMembro(Guid id, [FromBody] UpdateMembroDto dto)
        {
            var updated = await _userService.UpdateMembroEquipeAsync(id, dto);
            if (!updated) return BadRequest("Não foi possível atualizar o membro ou ele não existe.");
            return NoContent();
        }

        [HttpPost("{id}/roles/assign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRole(Guid id, [FromQuery] string roleName)
        {
            var assigned = await _userService.AssignRoleToMembroAsync(id, roleName);
            if (!assigned) return BadRequest($"Não foi possível atribuir a role '{roleName}'.");
            return NoContent();
        }

        [HttpPost("{id}/roles/remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveRole(Guid id, [FromQuery] string roleName)
        {
            var removed = await _userService.RemoveRoleFromMembroAsync(id, roleName);
            if (!removed) return BadRequest($"Não foi possível remover a role '{roleName}'.");
            return NoContent();
        }
    }
}