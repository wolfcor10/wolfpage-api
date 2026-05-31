namespace WolfPage.Api.Domain.Entities;

public class UserExternalLogin
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Provider { get; set; } = default!;
    public string ProviderUserId { get; set; } = default!;
    public string Email { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = default!;
}
