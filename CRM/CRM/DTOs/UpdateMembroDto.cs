namespace DTOs;

public class UpdateMembroDto
{
    public string NomeCompleto { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Cargo { get; set; }
    public bool Ativo { get; set; }
    public List<string> Roles { get; set; }
}