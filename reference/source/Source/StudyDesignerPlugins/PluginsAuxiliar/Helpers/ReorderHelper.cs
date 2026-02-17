namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ReorderHelper
    {
        public static IDictionary<Guid, int> ToSequentialOrder(IEnumerable<Guid> rows)
        {
            if (rows == null || rows.Count() == 0)
            {
                return null;
            }

            var sortOrder = 0;
            var result = new Dictionary<Guid, int>() { };
            foreach (var rowId in rows)
            {
                result.Add(rowId, sortOrder++);
            }

            return result;
        }
    }
}
