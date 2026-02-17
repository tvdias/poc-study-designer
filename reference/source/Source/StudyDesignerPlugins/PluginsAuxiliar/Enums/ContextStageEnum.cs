namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Enums
{
    /// <summary>
    /// Standardized pipeline stage values for Dataverse plugin execution.
    /// Matches Microsoft.Xrm.Sdk pipeline stages used in registrations.
    /// </summary>
    public enum ContextStageEnum
    {
        PreValidation = 10,
        PreOperation = 20,
        PostOperation = 40
    }
}
