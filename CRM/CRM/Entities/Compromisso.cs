using _Enums;

namespace Entities
{
    public class Compromisso
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; }
        public string Descricao { get; set; }
        public DateTime DataHoraInicio { get; set; }
        public DateTime DataHoraFim { get; set; }
        public TipoAtendimento TipoAtendimento { get; set; }
        public string LocalOuLink { get; set; }
        public Guid? CidadaoId { get; set; }
        public Cidadao Cidadao { get; set; }
        public Guid? ResponsavelId { get; set; } // Membro da equipe responsável pelo compromisso
        public MembroEquipe Responsavel { get; set; }
        public StatusCompromisso Status { get; set; } = StatusCompromisso.Pendente;
        public string Notas { get; set; }
    }
}