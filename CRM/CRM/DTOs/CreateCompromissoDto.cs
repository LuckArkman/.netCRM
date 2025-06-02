using _Enums;

namespace DTOs;

public class CreateCompromissoDto
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public TipoAtendimento TipoAtendimento { get; set; }
    public string LocalOuLink { get; set; }
    public Guid? CidadaoId { get; set; }
    public string CidadaoNome { get; set; } // Para quick create
    public Guid? ResponsavelId { get; set; }
    public string Notas { get; set; }
}