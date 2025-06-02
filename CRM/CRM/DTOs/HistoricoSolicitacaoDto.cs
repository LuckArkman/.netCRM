using System;
using _Enums;

namespace DTOs
{
    public class HistoricoSolicitacaoDto
    {
        public Guid Id { get; set; }
        public DateTime DataHora { get; set; }
        public string TipoEvento { get; set; } // String representação do enum
        public string Detalhes { get; set; }
        public Guid UsuarioResponsavelId { get; set; }
        public string UsuarioResponsavelNome { get; set; }
    }
}