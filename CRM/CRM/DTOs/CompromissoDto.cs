namespace DTOs;

public class CompromissoDto
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public DateTime DataHoraInicio { get; set; }
    public DateTime DataHoraFim { get; set; }
    public string TipoAtendimento { get; set; }
    public string LocalOuLink { get; set; }
    public Guid? CidadaoId { get; set; }
    public string NomeCidadao { get; set; }
    public Guid? ResponsavelId { get; set; }
    public string NomeResponsavel { get; set; }
    public string Status { get; set; }
    public string Notas { get; set; }
}