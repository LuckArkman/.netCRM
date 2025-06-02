namespace DTOs;

public class AuthResultDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; } // Se usar Refresh Tokens
    public DateTime Expiration { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public IList<string> Roles { get; set; }
    public List<string> Errors { get; set; }
    public bool IsSuccess { get; set; }
}