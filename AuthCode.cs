namespace cli;

internal class AuthCode
{
    public string verificationCode { get; set; }
    public int organizationId { get; set; }
    public string userId { get; set; }
    public DateTime createdAt { get; set; }
    public DateTime expiresAt { get; set; }
}
