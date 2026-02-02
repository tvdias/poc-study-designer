namespace Api.Models;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? IntegrationProperties { get; set; }
}
