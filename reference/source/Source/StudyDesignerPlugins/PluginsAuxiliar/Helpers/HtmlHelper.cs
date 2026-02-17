namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class HtmlHelper
    {
        public static string HtmlToPlainText(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            // Decode HTML entities (&amp;, &nbsp;, etc.)
            html = WebUtility.HtmlDecode(html);

            // Your data uses literal "\n" inside the <td>, turn that into real newlines
            html = html.Replace("\\n", "\n");

            var sb = new StringBuilder();

            // Find each table row
            var rowMatches = Regex.Matches(
                html,
                @"<tr[^>]*>(.*?)</tr>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match rowMatch in rowMatches)
            {
                var rowHtml = rowMatch.Groups[1].Value;

                // Find each cell (th or td)
                var cellMatches = Regex.Matches(
                    rowHtml,
                    @"<t[dh][^>]*>(.*?)</t[dh]>",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline);

                if (cellMatches.Count == 0)
                {
                    continue;
                }

                string StripTags(string input)
                {
                    // Convert <br> to newline before stripping other tags
                    input = Regex.Replace(input, @"<(br|BR)\s*/?>", "\n");
                    return Regex.Replace(input, "<.*?>", string.Empty);
                }

                var cell1 = StripTags(cellMatches[0].Groups[1].Value).Trim();

                if (cellMatches.Count > 1)
                {
                    var cell2 = StripTags(cellMatches[1].Groups[1].Value).Trim();
                    sb.AppendLine($"{cell1}: {cell2}");
                }
                else
                {
                    sb.AppendLine(cell1);
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
