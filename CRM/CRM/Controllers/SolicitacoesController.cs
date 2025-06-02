using DTOs;
using Interfaces;
using Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using _Enums;

namespace Controllers
{
    [Authorize] // Requer autenticação para todos os endpoints neste controller
    [ApiController]
    [Route("api/[controller]")]
    public class SolicitacoesController : ControllerBase
    {
        private readonly ISolicitacaoService _solicitacaoService;
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IHubContext<DashboardHub> _dashboardHub; // Para atualizar o dashboard

        public SolicitacoesController(ISolicitacaoService solicitacaoService,
                                       IHubContext<NotificationHub> notificationHub,
                                       IHubContext<DashboardHub> dashboardHub)
        {
            _solicitacaoService = solicitacaoService;
            _notificationHub = notificationHub;
            _dashboardHub = dashboardHub;
        }

        private Guid GetCurrentUserId()
        {
            // O ID do usuário autenticado vem do token JWT
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedAccessException("ID do usuário não encontrado no token.");
            }
            return Guid.Parse(userIdString);
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<SolicitacaoDto>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSolicitacoes(
            [FromQuery] CategoriaSolicitacao? categoria,
            [FromQuery] StatusSolicitacao? status,
            [FromQuery] PrioridadeSolicitacao? prioridade,
            [FromQuery] Guid? membroAtribuidoId,
            [FromQuery] string search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var solicitacoes = await _solicitacaoService.GetFilteredSolicitacoesAsync(
                categoria, status, prioridade, membroAtribuidoId, search, pageNumber, pageSize);
            return Ok(solicitacoes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SolicitacaoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSolicitacao(Guid id)
        {
            var solicitacao = await _solicitacaoService.GetSolicitacaoByIdAsync(id);
            if (solicitacao == null) return NotFound();
            return Ok(solicitacao);
        }

        [HttpGet("protocolo/{protocolo}")]
        [AllowAnonymous] // Pode ser público para consulta de cidadãos
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SolicitacaoDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSolicitacaoByProtocolo(string protocolo)
        {
            var solicitacao = await _solicitacaoService.GetSolicitacaoByProtocoloAsync(protocolo);
            if (solicitacao == null) return NotFound();
            return Ok(solicitacao);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Secretario,Atendente")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SolicitacaoDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateSolicitacao([FromBody] CreateSolicitacaoDto dto)
        {
            var solicitacao = await _solicitacaoService.CreateSolicitacaoAsync(dto, GetCurrentUserId());
            if (solicitacao == null) return BadRequest(new { message = "Não foi possível criar a solicitação." });

            // Notificar em tempo real sobre nova solicitação
            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new { message = $"Nova solicitação criada: {solicitacao.Protocolo}", type = "success" });
            await _dashboardHub.Clients.All.SendAsync("UpdateKpi", "newRequest", 1); // Exemplo: atualizar KPI de novas solicitações

            return CreatedAtAction(nameof(GetSolicitacao), new { id = solicitacao.Id }, solicitacao);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Secretario,Assessor,Atendente")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateSolicitacao(Guid id, [FromBody] UpdateSolicitacaoDto dto)
        {
            var updated = await _solicitacaoService.UpdateSolicitacaoAsync(id, dto, GetCurrentUserId());
            if (!updated) return NotFound();

            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new { message = $"Solicitação {id} atualizada.", type = "info" });
            // Se o status mudou para concluído, atualiza o KPI
            if (dto.Status == StatusSolicitacao.Concluido)
            {
                await _dashboardHub.Clients.All.SendAsync("UpdateKpi", "completedRequest", 1);
            }

            return NoContent();
        }

        [HttpPost("{id}/observacoes")]
        [Authorize(Roles = "Admin,Secretario,Assessor,Atendente")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddObservacao(Guid id, [FromBody] AddObservacaoDto dto)
        {
            var added = await _solicitacaoService.AddObservacaoAsync(id, dto, GetCurrentUserId());
            if (!added) return NotFound();

            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new { message = $"Nova observação adicionada à solicitação {id}.", type = "info" });
            return NoContent();
        }

        [HttpPost("{id}/anexos")]
        [Authorize(Roles = "Admin,Secretario,Assessor,Atendente")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UploadAnexo(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Nenhum arquivo enviado.");

            using var stream = file.OpenReadStream();
            var uploaded = await _solicitacaoService.AddAnexoAsync(id, stream, file.FileName, file.ContentType, GetCurrentUserId());

            if (!uploaded) return NotFound();

            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", new { message = $"Anexo '{file.FileName}' adicionado à solicitação {id}.", type = "info" });
            return Ok(new { message = "Anexo enviado com sucesso." });
        }

        [HttpGet("{id}/historico")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<HistoricoSolicitacaoDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSolicitacaoHistorico(Guid id)
        {
            var historico = await _solicitacaoService.GetSolicitacaoHistoricoAsync(id);
            if (historico == null) return NotFound();
            return Ok(historico);
        }
    }
}