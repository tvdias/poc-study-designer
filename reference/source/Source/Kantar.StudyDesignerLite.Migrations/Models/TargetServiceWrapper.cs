namespace Kantar.StudyDesignerLite.Migrations.Models;

using Microsoft.PowerPlatform.Dataverse.Client;

public class TargetServiceWrapper
{
    public IOrganizationServiceAsync Service { get; }

    public TargetServiceWrapper(IOrganizationServiceAsync service)
    {
        Service = service;
    }
}

