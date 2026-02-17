namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.Project
{
    using System;
    using Kantar.StudyDesignerLite.Plugins;

    public interface IProjectRepository
    {
        KT_Project Get(Guid projectId);
    }
}
