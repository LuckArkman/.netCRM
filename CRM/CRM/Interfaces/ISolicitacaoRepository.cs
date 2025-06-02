using _Enums;
using Entities;
using Interfaces;

namespace Interfaces
{
    public interface ISolicitacaoRepository : IRepository<Solicitacao>
    {
        Task<Solicitacao> GetByProtocoloAsync(string protocolo);
        Task<IEnumerable<Solicitacao>> GetFilteredSolicitacoesAsync(
            CategoriaSolicitacao? categoria = null,
            StatusSolicitacao? status = null,
            PrioridadeSolicitacao? prioridade = null,
            Guid? membroAtribuidoId = null,
            string searchString = null,
            int pageNumber = 1,
            int pageSize = 10);
        Task<IEnumerable<HistoricoSolicitacao>> GetHistoricoBySolicitacaoIdAsync(Guid solicitacaoId);
    }
}