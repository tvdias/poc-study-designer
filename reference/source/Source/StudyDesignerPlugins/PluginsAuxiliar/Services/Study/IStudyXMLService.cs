namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services
{
    using System;

    public interface IStudyXMLService
    {
        string GenerateAndStoreStudyXml(Guid studyId);
    }
}
