using _Enums;

namespace Entities
{
    public class HistoricoSolicitacao
    {
        public Guid Id { get; set; }
        public Guid SolicitacaoId { get; set; }
        public Solicitacao Solicitacao { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
        public TipoEventoHistorico TipoEvento { get; set; }
        public string Detalhes { get; set; } // Ex: "Status alterado de 'Registrado' para 'Em Análise'"
        public Guid UsuarioResponsavelId { get; set; }
        public MembroEquipe UsuarioResponsavel { get; set; }
    }
}