namespace DTOs;

public class ObservacaoDto
{
    public Guid Id { get; set; }
    public string Conteudo { get; set; }
    public DateTime DataRegistro { get; set; }
    public string AutorNome { get; set; }
}