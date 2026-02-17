namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Project
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class ProjectRepository : IProjectRepository
    {
        private readonly IOrganizationService _service;

        public ProjectRepository(IOrganizationService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public KT_Project Get(Guid projectId)
        {
            var entity = _service.Retrieve(
                KT_Project.EntityLogicalName,
                projectId,
                new ColumnSet(true));

            return entity.ToEntity<KT_Project>();
        }
    }
}
