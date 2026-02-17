using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers
{
    public static class EntityHelpers
    {
        /// <summary>
        /// Extracts GUIDs from a list of entities based on a specified lookup field.
        /// </summary>
        /// <param name="entities">The list of entities.</param>
        /// <param name="lookupFieldName">The schema name of the lookup field.</param>
        /// <returns>A list of GUIDs from the lookup field references.</returns>
        public static IList<Guid> JoinIds(
            IEnumerable<Entity> entities,
            string lookupFieldName)
        {
            var ids = new List<Guid>();

            foreach (var entity in entities)
            {
                if (entity.Contains(lookupFieldName) && entity[lookupFieldName] is Guid id)
                {
                    ids.Add(id);
                }
            }

            return ids;
        }
    }
}
