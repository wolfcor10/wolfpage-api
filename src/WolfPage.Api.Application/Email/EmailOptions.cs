namespace WolfPage.Api.Application.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Provider { get; set; } = "Log";
    public string FromAddress { get; set; } = "donotreply@wolfpage.local";
}
