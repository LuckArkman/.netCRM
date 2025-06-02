using _Enums;
using DTOs;

namespace Interfaces;

public interface ISolicitacaoService
{
    Task<SolicitacaoDto> CreateSolicitacaoAsync(CreateSolicitacaoDto dto, Guid currentUserId);
    Task<SolicitacaoDto> GetSolicitacaoByIdAsync(Guid id);
    Task<SolicitacaoDto> GetSolicitacaoByProtocoloAsync(string protocolo);
    Task<IEnumerable<SolicitacaoDto>> GetFilteredSolicitacoesAsync(
        CategoriaSolicitacao? categoria = null,
        StatusSolicitacao? status = null,
        PrioridadeSolicitacao? prioridade = null,
        Guid? membroAtribuidoId = null,
        string searchString = null,
        int pageNumber = 1,
        int pageSize = 10);
        
    // --- ADICIONE ESTAS DUAS LINHAS FALTANTES ---
    Task<bool> UpdateSolicitacaoAsync(Guid id, UpdateSolicitacaoDto dto, Guid currentUserId);
    Task<bool> AddObservacaoAsync(Guid solicitacaoId, AddObservacaoDto dto, Guid currentUserId);
    Task<bool> AddAnexoAsync(Guid solicitacaoId, Stream fileStream, string fileName, string contentType, Guid currentUserId);
    Task<IEnumerable<HistoricoSolicitacaoDto>> GetSolicitacaoHistoricoAsync(Guid solicitacaoId);
    // --- FIM DAS LINHAS A SEREM ADICIONADAS ---
}