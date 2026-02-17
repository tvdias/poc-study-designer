using System;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    public static class PluginErrorHelper
    {
        /// <summary>
        /// Unwraps the innermost exception message in a safe and clean way.
        /// </summary>
        /// <param name="ex">The thrown exception.</param>
        /// <returns>Cleaned error message from the deepest exception.</returns>
        public static string GetInnermostMessage(Exception ex)
        {
            if (ex == null)
            {
                return "An unknown error occurred.";
            }

            Exception current = ex;

            // Drill down to innermost exception
            while (current.InnerException != null)
            {
                current = current.InnerException;
            }

            return CleanMessage(current.Message);
        }

        /// <summary>
        /// Optional: Clean known technical messages or noise.
        /// </summary>
        /// <param name="rawMessage">The raw error message.</param>
        /// <returns>Sanitized error string.</returns>
        public static string CleanMessage(string rawMessage)
        {
            if (string.IsNullOrWhiteSpace(rawMessage))
            {
                return "An unexpected error occurred.";
            }

            // Add message cleanups here if needed
            if (rawMessage.Contains("Kantar.StudyDesignerLite.Plugins"))
            {
                rawMessage = rawMessage.Replace("An error occurred executing Plugin Kantar.StudyDesignerLite.Plugins.", "");
            }

            return rawMessage.Trim();
        }
    }
}
