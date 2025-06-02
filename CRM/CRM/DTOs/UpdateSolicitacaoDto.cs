using _Enums;

namespace DTOs;

public class UpdateSolicitacaoDto
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public CategoriaSolicitacao? Categoria { get; set; }
    public PrioridadeSolicitacao? Prioridade { get; set; }
    public StatusSolicitacao? Status { get; set; }
    public Guid? MembroAtribuidoId { get; set; }
    public DateTime? PrazoEstimadoConclusao { get; set; }
    public List<string> Tags { get; set; } = new List<string>(); // Pode ser sobreescrito ou adicionar/remover
}