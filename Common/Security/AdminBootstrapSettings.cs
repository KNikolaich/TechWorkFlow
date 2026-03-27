namespace Common.Security;

public sealed class AdminBootstrapSettings
{
    public const string SectionName = "AdminBootstrap";

    public string UserName { get; set; } = "admin";
    public string Email { get; set; } = "admin@localhost";
    public string Password { get; set; } = "Admin123!";
    public string FullName { get; set; } = "System Administrator";
}
