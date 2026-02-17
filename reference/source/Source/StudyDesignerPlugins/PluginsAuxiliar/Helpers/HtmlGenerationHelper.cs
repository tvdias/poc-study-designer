namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using Kantar.StudyDesignerLite.Plugins;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedList;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Models.Subset;
    using Microsoft.Xrm.Sdk;

    public static class HtmlGenerationHelper
    {
        private static readonly IReadOnlyDictionary<string, string> s_fieldsToInclude = new Dictionary<string, string>
        {
            { "Row Sort Order", KT_QuestionnaireLines.Fields.KTR_RowSortOrder },
            { "Column Sort Order", KT_QuestionnaireLines.Fields.KTR_ColumnSortOrder },
            { "Answer Min", KT_QuestionnaireLines.Fields.KTR_AnswerMin },
            { "Answer Max", KT_QuestionnaireLines.Fields.KTR_AnswerMax },
            { "Question Format Details", KT_QuestionnaireLines.Fields.KTR_QuestionFormatDetails },
            { "Scripter Notes", KT_QuestionnaireLines.Fields.KTR_ScripterNotes },
            { "Custom Notes", KT_QuestionnaireLines.Fields.KTR_CustomNotes }
        };

        private const string HtmlStyles = "<div style='font-family:Arial;font-size:9pt;'>";
        private const string HtmlTableStart = "<table cellpadding='0' cellspacing='0' style='border-collapse:collapse; border:1px solid #ccc !important; table-layout:fixed; width:100%;'>";
        private const string HtmlTableEnd = "</table></div>";
        private const string HtmlTableRow = "<tr><td style='padding:4px; font-weight:bold; width:20%; max-width:50px; border-collapse:collapse; border:1px solid #ccc !important;'>{0}</td>" +
                            "<td style='padding:4px; width:80%; border-collapse:collapse; border:1px solid #ccc !important;white-space:pre-line;'>{1}</td></tr>";
        private const string HtmlTitle = "<div style='text-align:left; font-weight:bold; margin:10px 0 4px 0;'>{0}</div>";

        private const string StyleFirstColumn = "style='padding:4px; width:350px; max-width:350px; word-wrap:break-word; white-space:normal; border:1px solid #ccc; font-family:Arial;'";
        private const string StyleSecondColumn = "style='padding:4px; width:200px; max-width:200px; word-wrap:break-word; white-space:normal; border:1px solid #ccc; font-family:Arial;'";
        private const string StyleThirdColumn = "style='padding:4px; width:200px; max-width:200px; word-wrap:break-word; white-space:normal; border:1px solid #ccc; font-family:Arial;'";

        public static IReadOnlyDictionary<string, string> GetFieldsToInclude()
        {
            return s_fieldsToInclude;
        }

        public static string GenerateScripterNotesHtml(Entity question)
        {
            string safeValue = string.Empty;
            var sb = new StringBuilder();
            sb.Append(HtmlStyles);
            sb.Append(HtmlTableStart);

            var fieldsToInclude = GetFieldsToInclude();

            foreach (var fields in fieldsToInclude)
            {
                var label = fields.Key;
                var fieldName = fields.Value;

                if (question.Attributes.Contains(fieldName) && question[fieldName] != null)
                {
                    string value = question.FormattedValues.Contains(fieldName)
                        ? question.FormattedValues[fieldName]
                        : question[fieldName]?.ToString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {

                        if (fieldName == KT_QuestionnaireLines.Fields.KTR_ScripterNotes || fieldName == KT_QuestionnaireLines.Fields.KTR_CustomNotes || fieldName == KT_QuestionnaireLines.Fields.KTR_QuestionFormatDetails)
                        {   // Replace paragraph breaks with HTML <br/><br/> for visible paragraph spacing
                            safeValue = AddLineBreaksAndFormat(value);
                        }
                        else
                        {
                            safeValue = System.Web.HttpUtility.HtmlEncode(value);
                        }
                        sb.AppendFormat(
                            HtmlTableRow,
                            label, safeValue
                        );
                    }
                }
            }

            sb.Append(HtmlTableEnd);
            return sb.ToString();
        }

        public static string GenerateAnswerListHtml(
            IEnumerable<KTR_QuestionnaireLinesAnswerList> answers,
            IEnumerable<KTR_ManagedList> managedListsRows,
            IEnumerable<KTR_ManagedList> managedListsColumns)
        {
            var htmlTableStart = "<table cellpadding='0' cellspacing='0' style='font-family:Arial; border-collapse:collapse; border:1px solid #ccc; table-layout:fixed; width:600px;'>";

            var sb = new StringBuilder();
            sb.Append(HtmlStyles);

            // --- ROW ANSWERS TABLE ---
            var answerRows = answers
                .Where(a => a.KTR_AnswerType == KTR_AnswerType.Row || a.KTR_AnswerType == null)
                .OrderBy(a => a.KTR_DisplayOrder)
                .ToList();

            if (answerRows.Any() || managedListsRows.Any())
            {
                sb.AppendFormat(HtmlTitle, "Rows");
                sb.Append(htmlTableStart);

                BuildManagedListInfo(managedListsRows, sb);

                BuildAnswerListInfo(answerRows, sb);

                sb.Append("</table>");
            }

            // --- COLUMN ANSWERS TABLE ---
            var answerCols = answers.Where(a => a.KTR_AnswerType == KTR_AnswerType.Column)
                              .OrderBy(a => a.KTR_DisplayOrder).ToList();

            if (answerCols.Any() || managedListsColumns.Any())
            {
                sb.AppendFormat(HtmlTitle, "Columns");
                sb.Append(htmlTableStart);

                BuildManagedListInfo(managedListsColumns, sb);

                BuildAnswerListInfo(answerCols, sb);

                sb.Append(HtmlTableEnd);
            }

            return sb.ToString();
        }

        public static string AddLineBreaksAndFormat(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            // 0. Remove script tags and their content
            value = Regex.Replace(value, @"<script\b[^>]*>.*?</script>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            // 1. Standardize Smart Quotes
            value = value.Replace("‘", "'")
                         .Replace("’", "'")
                         .Replace("“", "\"")
                         .Replace("”", "\"");

            // 2. Replace any number of backslashes before unicode escapes with the actual character
            // Handles: \u0085, \\u0085, etc. Now also handles \u000A, \u000D, \u000B, \u000C, \u200B
            value = Regex.Replace(value, @"(\\+)(u0085)", m => "\u0085");
            value = Regex.Replace(value, @"(\\+)(u2028)", m => "\u2028");
            value = Regex.Replace(value, @"(\\+)(u2029)", m => "\u2029");
            value = Regex.Replace(value, @"(\\+)(u000A)", m => "\u000A");
            value = Regex.Replace(value, @"(\\+)(u000D)", m => "\u000D");
            value = Regex.Replace(value, @"(\\+)(u000B)", m => "\u000B");
            value = Regex.Replace(value, @"(\\+)(u000C)", m => "\u000C");
            value = Regex.Replace(value, @"(\\+)(u200B)", m => "\u200B");
            value = Regex.Replace(value, @"(\\+)(r)", m => "\r");
            value = Regex.Replace(value, @"(\\+)(n)", m => "\n");
            value = Regex.Replace(value, @"(\\+)(r\n)", m => "\n");

            // 3. Encode CONTENT, but preserve allowed style tags
            string encoded = WebUtility.HtmlEncode(value);
            encoded = Regex.Replace(encoded, @"&lt;(\/?)(b|i|u|strong|em)&gt;", "<$1$2>", RegexOptions.IgnoreCase);

            // 4. Normalize Real Line Endings (CRLF -> LF)
            encoded = encoded.Replace("\r\n", "\n").Replace("\r", "\n");

            // 5. Paragraph Separator (\u2029) to <br/><br/>
            encoded = encoded.Replace("\u2029", "<br/><br/>");

            // 6. Line Separators to <br/>
            encoded = encoded.Replace("\n", "<br/>")
                             .Replace("\u0085", "<br/>")
                             .Replace("\u2028", "<br/>")
                             .Replace("\u000A", "<br/>")
                             .Replace("\u000D", "<br/>")
                             .Replace("\u000B", "<br/>")
                             .Replace("\u000C", "<br/>")
                             .Replace("\u200B", "<br/>");

            // 7. Apply Stylistic Formatting (Sentence Spacing)
            encoded = Regex.Replace(
                encoded,
                @"(?<=[.!?])\s*(?=[A-Z])", "<br/><br/>",
                RegexOptions.None,
                TimeSpan.FromMilliseconds(500));

            // Ensure wrapped in <p> for valid HTML
            if (!encoded.StartsWith("<p>")) encoded = "<p>" + encoded;
            if (!encoded.EndsWith("</p>")) encoded += "</p>";

            return encoded;
        }
        private static void BuildAnswerListInfo(IEnumerable<KTR_QuestionnaireLinesAnswerList> answers, StringBuilder sb)
        {
            if (!answers.Any())
            {
                return;
            }

            foreach (var row in answers)
            {
                var answerText = System.Web.HttpUtility.HtmlEncode(row.KTR_AnswerText);
                var answerCode = System.Web.HttpUtility.HtmlEncode(row.KTR_AnswerCode);

                var flagList = BuildAnswerFlagsText(row);

                var flagText = string.Join(", ", flagList);

                BuildTableRow(sb, answerText, answerCode, flagText);
            }
        }

        private static void BuildManagedListInfo(IEnumerable<KTR_ManagedList> managedLists, StringBuilder sb)
        {
            if (!managedLists.Any())
            {
                return;
            }

            foreach (var mlRow in managedLists)
            {
                var firstColumnText = $"Managed List: {mlRow.KTR_Name}";
                BuildTableRow(sb, firstColumnText, string.Empty, string.Empty);
            }
        }

        private static void BuildTableRow(StringBuilder sb, string firstColumnsText, string secondColumnText, string thirdColumnText)
        {
            sb.AppendFormat("<tr><td {0}>{1}</td><td {2}>{3}</td><td {4}>{5}</td></tr>",
                        StyleFirstColumn, firstColumnsText, StyleSecondColumn, secondColumnText, StyleThirdColumn, thirdColumnText);
        }

        private static List<string> BuildAnswerFlagsText(KTR_QuestionnaireLinesAnswerList answer)
        {
            var flagList = new List<string>();
            if (answer.KTR_IsFixed == true) { flagList.Add("FIXED"); }
            if (answer.KTR_IsExclusive == true) { flagList.Add("EXCLUSIVE"); }
            if (answer.KTR_IsOpen == true) { flagList.Add("OPEN"); }
            return flagList;
        }

        public static string RebuildProjectManagedListsHtml(
            ManagedListRepository managedListRepository,
            ITracingService tracing,
            Guid projectId)
        {
            var managedLists = managedListRepository.GetByProjectId(projectId);
            tracing?.Trace($"HtmlGenerationHelper.RebuildProjectManagedListsHtml: Project={projectId} ActiveManagedListCount={managedLists.Count}");
            if (managedLists.Count == 0)
            {
                tracing?.Trace("No managed lists found. Cleared HTML.");
                return string.Empty;
            }

            var blocks = new List<string>();
            foreach (var ml in managedLists)
            {
                tracing?.Trace($"Processing ManagedList Id={ml.Id} Name='{ml.KTR_Name}' Auto={(ml.GetAttributeValue<bool?>(KTR_ManagedList.Fields.KTR_IsAutoGenerated) == true)}");
                var questionCount = managedListRepository.GetQuestionUsageCount(ml.Id, projectId);
                tracing?.Trace($"Question usage count for ManagedList {ml.Id} = {questionCount}");
                blocks.Add(BuildManagedListTable(ml, questionCount));
            }

            var finalHtml = string.Join("<br/>", blocks.Where(b => !string.IsNullOrEmpty(b)));
            tracing?.Trace($"Final HTML length={finalHtml.Length}");
            tracing?.Trace("Managed lists HTML generation completed (entities omitted).");
            return finalHtml ?? string.Empty;
        }

        public static string BuildManagedListTable(KTR_ManagedList ml, int questionCount)
        {
            if (questionCount == 0)
            {
                return string.Empty;
            }

            var name = ml?.KTR_Name ?? string.Empty;
            var isAuto = ml != null && ml.GetAttributeValue<bool?>(KTR_ManagedList.Fields.KTR_IsAutoGenerated) == true ? "Autogenerated" : "Custom";
            var sb = new System.Text.StringBuilder();
            sb.Append("<div style='display: inline-block; margin-right:80px;'><h4>Managed List : ")
              .Append(Escape(name))
              .Append("</h4></div>");
            sb.Append("<div style='display: inline-block; margin-right:80px;'><p>")
              .Append(Escape(isAuto))
              .Append("</p></div>");
            sb.Append("<div style='display: inline-block;'><p>Question Count : ")
              .Append(questionCount)
              .Append("</p></div>");
            return sb.ToString();
        }

        public static string GenerateAnswerSubsetListHtml(
            IEnumerable<KTR_QuestionnaireLinesAnswerList> answers,
            IEnumerable<QuestionnaireLineSubsetWithLocation> subsetsAsRows,
            IEnumerable<QuestionnaireLineSubsetWithLocation> subsetsAsColumns)
        {
            var htmlTableStart = "<table cellpadding='0' cellspacing='0' style='font-family:Arial; border-collapse:collapse; border:1px solid #ccc; table-layout:fixed; width:600px;'>";

            var sb = new StringBuilder();
            sb.Append(HtmlStyles);

            // --- ROW SECTION ---
            var answerRows = answers
                .Where(a => a.KTR_AnswerType == KTR_AnswerType.Row || a.KTR_AnswerType == null)
                .OrderBy(a => a.KTR_DisplayOrder)
                .ToList();

            var rowSubsets = (subsetsAsRows ?? Enumerable.Empty<QuestionnaireLineSubsetWithLocation>()).ToList();

            if (answerRows.Any() || rowSubsets.Any())
            {
                sb.AppendFormat(HtmlTitle, "Rows");
                sb.Append(htmlTableStart);

                BuildSubsetInfo(rowSubsets, sb);

                BuildAnswerListInfo(answerRows, sb);

                sb.Append("</table>");
            }

            // --- COLUMN SECTION ---
            var answerCols = answers
                .Where(a => a.KTR_AnswerType == KTR_AnswerType.Column)
                .OrderBy(a => a.KTR_DisplayOrder)
                .ToList();

            var colSubsets = (subsetsAsColumns ?? Enumerable.Empty<QuestionnaireLineSubsetWithLocation>()).ToList();

            if (answerCols.Any() || colSubsets.Any())
            {
                sb.AppendFormat(HtmlTitle, "Columns");
                sb.Append(htmlTableStart);

                BuildSubsetInfo(colSubsets, sb);

                BuildAnswerListInfo(answerCols, sb);

                sb.Append(HtmlTableEnd);
            }

            return sb.ToString();
        }

        private static void BuildSubsetInfo(IEnumerable<QuestionnaireLineSubsetWithLocation> subsets, StringBuilder sb)
        {
            if (subsets == null || !subsets.Any())
            {
                return;
            }

            foreach (var subset in subsets)
            {
                var subsetName = System.Web.HttpUtility.HtmlEncode(subset.SubsetName ?? string.Empty);
                //var managedListName = System.Web.HttpUtility.HtmlEncode(subset.ManagedListName ?? string.Empty);
                var firstColumnText = $"Subset: {subsetName}";
                //var secondColumnText = string.IsNullOrEmpty(managedListName) ? string.Empty : $"Managed List: {managedListName}";
                BuildTableRow(sb, firstColumnText, "", string.Empty);
            }
        }

        // ========================= Subset Snapshot HTML =========================

        public static string RenderSubsetSnapshotView(IDictionary<string, SubsetSnapshotSummary> subsetSnapshotView)
        {
            if (subsetSnapshotView == null || subsetSnapshotView.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            foreach (var kvp in subsetSnapshotView
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var vm = kvp.Value ?? new SubsetSnapshotSummary
                {
                    SubsetName = kvp.Key,
                    QuestionCount = 0,
                    Entities = new List<SubsetSnapshotEntity>()
                };

                var name = string.IsNullOrWhiteSpace(vm.SubsetName) ? kvp.Key : vm.SubsetName;

                // Header row: "Sublist: <name>" (left) and "Question Count : <n>" (right)
                sb.Append("<div class='subset-snapshot' style='margin:8px 0;'>");
                sb.Append("<table style='width:100%; border-collapse:collapse;'>");
                sb.Append("<tr>");
                sb.Append("<td style='font-family:Segoe UI,Arial,sans-serif;font-size:18px;'>Sublist: ");
                sb.Append(Escape(name));
                sb.Append("</td>");
                sb.Append("<td style='font-family:Segoe UI,Arial,sans-serif;font-size:18px;text-align:right;'>Question Count : ");
                sb.Append(vm.QuestionCount);
                sb.Append("</td>");
                sb.Append("</tr>");
                sb.Append("</table>");

                // Entities 2-column table: Name | Code
                sb.Append(BuildEntitiesSnapshotTable(vm.Entities));

                sb.Append("</div>");
                sb.Append("<br />");
            }

            return sb.ToString();
        }

        private static string BuildEntitiesSnapshotTable(IList<SubsetSnapshotEntity> entities)
        {
            var rows = new StringBuilder();

            if (entities != null && entities.Count > 0)
            {
                foreach (var item in entities)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    rows.Append("<tr>");
                    rows.Append("<td style='padding:3px 6px;border:1px solid #ddd;font-family:Segoe UI,Arial,sans-serif;font-size:18px;'>");
                    rows.Append(Escape(item.Name));
                    rows.Append("</td>");
                    rows.Append("<td style='padding:3px 6px;border:1px solid #ddd;font-family:Segoe UI,Arial,sans-serif;font-size:18px;'>");
                    rows.Append(Escape(item.Code));
                    rows.Append("</td>");
                    rows.Append("</tr>");
                }
            }
            else
            {
                rows.Append("<tr><td style='padding:3px 6px;border:1px solid #ddd;font-family:Segoe UI,Arial,sans-serif;font-size:18px;' colspan='2'>(No entities)</td></tr>");
            }

            return "<table style='border-collapse:collapse; margin-top:4px; font-family:Segoe UI,Arial,sans-serif; font-size:18px;'>" +
                   rows.ToString() +
                   "</table>";
        }

        private static string Escape(string value) => value == null ? string.Empty : System.Security.SecurityElement.Escape(value);
    }
}
