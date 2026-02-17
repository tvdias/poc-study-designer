namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using System;

    public static class SubsetHtmlHelper
    {
        public static string BuildSubsetDefinitionTable(string subsetName, List<KTR_SubsetEntities> subsetEntities)
        {
            if (string.IsNullOrWhiteSpace(subsetName))
            {
                return string.Empty;
            }

            var rows = new List<string>();
            if (subsetEntities != null && subsetEntities.Count > 0)
            {
                var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var e in subsetEntities
                    .Where(x => x != null)
                    .OrderBy(x => x.KTR_Name))
                {
                    var name = e.KTR_Name ?? string.Empty;

                    // Skip duplicate names (case-insensitive)
                    if (!seenNames.Add(name))
                    {
                        continue;
                    }

                    rows.Add($"<tr><td>{Escape(name)}</td></tr>");
                }

                if (rows.Count == 0)
                {
                    rows.Add("<tr><td>(No unique entities)</td></tr>");
                }
            }
            else
            {
                rows.Add("<tr><td>(No entities)</td></tr>");
            }

            return $"<table border='1' cellspacing='0' cellpadding='3'><tr><th>{Escape(subsetName)}</th></tr>{string.Join(string.Empty, rows)}</table>";
        }

        private static string Escape(string value) => value == null ? string.Empty : System.Security.SecurityElement.Escape(value);
    }
}
