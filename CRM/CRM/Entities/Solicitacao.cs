using _Enums;

namespace Entities;

public class Solicitacao
{
    public Guid Id { get; set; } // Interno, GUID
    public string Protocolo { get; set; } // VR2024001
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public CategoriaSolicitacao Categoria { get; set; }
    public PrioridadeSolicitacao Prioridade { get; set; }
    public StatusSolicitacao Status { get; set; }
    public Guid? CidadaoId { get; set; }
    public Cidadao Cidadao { get; set; }
    public Guid? MembroAtribuidoId { get; set; }
    public MembroEquipe MembroAtribuido { get; set; }
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
    public DateTime DataUltimaAtualizacao { get; set; } = DateTime.UtcNow;
    public DateTime? PrazoEstimadoConclusao { get; set; }

    public ICollection<Anexo> Anexos { get; set; } = new List<Anexo>();
    public ICollection<Observacao> Observacoes { get; set; } = new List<Observacao>();
    public ICollection<TagSolicitacao> Tags { get; set; } = new List<TagSolicitacao>();
}