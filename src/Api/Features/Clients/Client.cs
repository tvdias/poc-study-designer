using Api.Features.Shared;

namespace Api.Features.Clients;

public class Client : AuditableEntity
{
    public Guid Id { get; set; }
    public required string AccountName { get; set; }
    public string? CompanyNumber { get; set; }
    public string? CustomerNumber { get; set; }
    public string? CompanyCode { get; set; }
    public bool IsActive { get; set; } = true;
}
