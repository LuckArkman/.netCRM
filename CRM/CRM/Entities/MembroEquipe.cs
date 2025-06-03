using Microsoft.AspNetCore.Identity; // Usamos Identity para gestão de usuários

namespace Entities
{
    // Extendemos IdentityUser para aproveitar a gestão de usuários do ASP.NET Core Identity
    public class MembroEquipe : IdentityUser<Guid> // Usamos Guid para a PK de usuários
    {
        public string NomeCompleto { get; set; }
        public string Cargo { get; set; }
        public string DataCadastro { get; set; } = DateTime.UtcNow.ToString();
        public bool Ativo { get; set; } = true;

        // Propriedades de navegação podem ser úteis para auditoria ou logs
        // public ICollection<Solicitacao> SolicitacoesAtribuidas { get; set; } // Se quisermos essa relação
    }
}