using Api.Features.Clients;
using Api.Features.Products;

namespace Api.Features.Projects;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }
    public string? Owner { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.Active;
    public bool CostManagementEnabled { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string? ModifiedBy { get; set; }
}

public enum ProjectStatus
{
    Active,
    OnHold,
    Closed
}
