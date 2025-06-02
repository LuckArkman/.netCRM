using _Enums;
using Data;
using Entities;
using Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repositories;

public class SolicitacaoRepository : Repository<Solicitacao>, ISolicitacaoRepository
{
    public SolicitacaoRepository(GabineteDigitalDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Solicitacao> GetByIdAsync(Guid id)
    {
        return await _dbContext.Solicitacoes
            .Include(s => s.Cidadao) // Inclui os dados do Cidadão
            .Include(s => s.MembroAtribuido) // Inclui os dados do Membro da Equipe atribuído
            .Include(s => s.Anexos) // Inclui a lista de Anexos
            .Include(s => s.Observacoes) // Inclui a lista de Observações
            .ThenInclude(o => o.Autor) // E para cada Observação, inclui o Autor (MembroEquipe)
            .Include(s => s.Tags) // Inclui a lista de Tags
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Solicitacao> GetByProtocoloAsync(string protocolo)
    {
        return await _dbContext.Solicitacoes
            .Include(s => s.Cidadao)
            .Include(s => s.MembroAtribuido)
            .Include(s => s.Anexos)
            .Include(s => s.Observacoes)
            .ThenInclude(o => o.Autor)
            .Include(s => s.Tags)
            .FirstOrDefaultAsync(s => s.Protocolo == protocolo);
    }

    public async Task<IEnumerable<Solicitacao>> GetFilteredSolicitacoesAsync(
        CategoriaSolicitacao? categoria = null,
        StatusSolicitacao? status = null,
        PrioridadeSolicitacao? prioridade = null,
        Guid? membroAtribuidoId = null,
        string searchString = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        IQueryable<Solicitacao> query = _dbContext.Solicitacoes
            .Include(s => s.Cidadao) // Inclui o cidadão para exibir na lista ou filtrar por nome
            .Include(s => s.MembroAtribuido) // Inclui o membro atribuído
            .Include(s => s.Tags); // Inclui as tags para exibição ou filtro

        // Aplicação dos filtros opcionais
        if (categoria.HasValue)
        {
            query = query.Where(s => s.Categoria == categoria.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        if (prioridade.HasValue)
        {
            query = query.Where(s => s.Prioridade == prioridade.Value);
        }

        if (membroAtribuidoId.HasValue)
        {
            query = query.Where(s => s.MembroAtribuidoId == membroAtribuidoId.Value);
        }

        // Filtro por string de busca (case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            var lowerSearchString = searchString.ToLower();
            query = query.Where(s =>
                s.Titulo.ToLower().Contains(lowerSearchString) ||
                s.Descricao.ToLower().Contains(lowerSearchString) ||
                s.Protocolo.ToLower().Contains(lowerSearchString) ||
                (s.Cidadao != null && s.Cidadao.Nome.ToLower().Contains(lowerSearchString)) ||
                s.Tags.Any(t => t.Tag.ToLower().Contains(lowerSearchString))
            );
        }

        // Ordenação (ex: por data de registro mais recente)
        query = query.OrderByDescending(s => s.DataRegistro);

        // Paginação
        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém o histórico completo de atualizações de uma solicitação.
    /// </summary>
    /// <param name="solicitacaoId">O ID da solicitação.</param>
    /// <returns>Uma coleção de eventos de histórico ordenados por data.</returns>
    public async Task<IEnumerable<HistoricoSolicitacao>> GetHistoricoBySolicitacaoIdAsync(Guid solicitacaoId)
    {
        return await _dbContext.HistoricoSolicitacoes
            .Where(h => h.SolicitacaoId == solicitacaoId)
            .Include(h => h.UsuarioResponsavel) // Inclui os dados do usuário que fez a alteração
            .OrderBy(h => h.DataHora) // Ordena pelo tempo para formar a linha do tempo
            .ToListAsync();
    }
}