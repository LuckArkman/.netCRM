using _Enums;

namespace DTOs;

public class UpdateCompromissoDto
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTime? DataHoraInicio { get; set; }
    public DateTime? DataHoraFim { get; set; }
    public TipoAtendimento? TipoAtendimento { get; set; }
    public string LocalOuLink { get; set; }
    public Guid? CidadaoId { get; set; }
    public Guid? ResponsavelId { get; set; }
    public StatusCompromisso? Status { get; set; }
    public string Notas { get; set; }
}