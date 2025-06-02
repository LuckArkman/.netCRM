namespace Entities;

public class TagSolicitacao
{
    public Guid Id { get; set; }
    public Guid SolicitacaoId { get; set; }
    public Solicitacao Solicitacao { get; set; }
    public string Tag { get; set; }
}