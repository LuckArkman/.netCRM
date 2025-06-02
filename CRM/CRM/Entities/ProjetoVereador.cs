namespace Entities
{
    public class ProjetoVereador
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public string StatusAtual { get; set; } // Ex: "Em votação", "Aprovado", "Em discussão"
        public string Area { get; set; } // Ex: "Educação", "Saúde", "Infraestrutura"
        public DateTime DataCriacao { get; set; }
        public DateTime? DataUltimaAtualizacao { get; set; }
    }
}