using _Enums;

namespace DTOs;

public class CreateSolicitacaoDto
{
    public string Titulo { get; set; }
    public string Descricao { get; set; }
    public CategoriaSolicitacao Categoria { get; set; }
    public PrioridadeSolicitacao Prioridade { get; set; }
    public Guid? CidadaoId { get; set; }
    public string CidadaoNome { get; set; } // Para criar/vincular cidadão rapidamente
    public string CidadaoEmail { get; set; }
    public string CidadaoTelefone { get; set; }
    public string CidadaoEndereco { get; set; }
    public string CidadaoBairro { get; set; }
    public string CidadaoCpf { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    // Anexos iniciais podem ser enviados via MultipartForm ou URL
}