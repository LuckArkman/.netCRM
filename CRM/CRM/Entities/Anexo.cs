namespace Entities
{
    public class Anexo
    {
        public Guid Id { get; set; }
        public Guid SolicitacaoId { get; set; }
        public Solicitacao Solicitacao { get; set; }
        public string NomeArquivo { get; set; }
        public string Url { get; set; } // URL do arquivo no Blob Storage
        public string TipoConteudo { get; set; }
        public DateTime DataUpload { get; set; } = DateTime.UtcNow;
        public Guid UploadedByUserId { get; set; } // Quem fez o upload
    }
}