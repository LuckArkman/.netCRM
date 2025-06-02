namespace Entities
{
    public class Observacao
    {
        public Guid Id { get; set; }
        public Guid SolicitacaoId { get; set; }
        public Solicitacao Solicitacao { get; set; }
        public string Conteudo { get; set; }
        public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
        public Guid AutorId { get; set; } // Quem registrou a observação
        public MembroEquipe Autor { get; set; }
    }
}