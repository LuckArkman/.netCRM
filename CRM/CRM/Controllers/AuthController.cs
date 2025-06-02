using System.Security.Claims;
using DTOs;
using GabineteDigital.Application.Interfaces;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GabineteDigital.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _userService.LoginAsync(loginDto);
            if (!result.IsSuccess)
            {
                return Unauthorized(result); // Retorna o DTO com erros
            }
            return Ok(result);
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")] // Apenas administradores podem registrar novos membros
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResultDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterMembroDto registerDto)
        {
            var result = await _userService.RegisterMembroEquipeAsync(registerDto);
            if (!result.IsSuccess)
            {
                return BadRequest(result); // Retorna o DTO com erros
            }
            return Ok(result);
        }

        // Exemplo: Endpoint para obter informações do usuário logado (usado pelo Laravel)
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MembroEquipeDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var user = await _userService.GetMembroEquipeByIdAsync(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}