namespace Entities
{
    public class Cidadao
    {
        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; } // Poderia ser um Value Object ou entidade mais complexa
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string CPF { get; set; } // Cuidado com LGPD
        public ICollection<Solicitacao> Solicitacoes { get; set; } = new List<Solicitacao>();
    }
}