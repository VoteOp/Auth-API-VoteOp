namespace VoteOp.AuthApi.Models;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";
    public DateTime CreatedUtc { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}