namespace DTOs;

public class RegisterMembroDto
{
    public string NomeCompleto { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Cargo { get; set; }
    public string PhoneNumber { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
}