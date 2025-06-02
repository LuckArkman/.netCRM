namespace DTOs;

public class SolicitacaoDto
{
    public Guid Id { get; set; }
    public string Protocolo { get; set; }
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public string Categoria { get; set; } // Representação string do Enum
    public string Prioridade { get; set; }
    public string Status { get; set; }
    public string NomeCidadao { get; set; }
    public string NomeMembroAtribuido { get; set; }
    public DateTime DataRegistro { get; set; }
    public DateTime DataUltimaAtualizacao { get; set; }
    public DateTime? PrazoEstimadoConclusao { get; set; }
    public List<AnexoDto> Anexos { get; set; } = new List<AnexoDto>();
    public List<ObservacaoDto> Observacoes { get; set; } = new List<ObservacaoDto>();
    public List<string> Tags { get; set; } = new List<string>();
}