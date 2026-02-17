namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Kantar.StudyDesignerLite.Plugins;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    [ExcludeFromCodeCoverage]
    public class ManagedListEntityRepository : IManagedListEntityRepository
    {
        private readonly IOrganizationService _service;

        public ManagedListEntityRepository(IOrganizationService service)
        {
            _service = service;
        }

        public List<KTR_ManagedListEntity> GetByManagedListId(Guid managedListId, string[] columns = null)
        {
            if (columns == null || columns.Length == 0)
            {
                columns = new string[]
                {
                    KTR_ManagedListEntity.Fields.Id,
                    KTR_ManagedListEntity.Fields.KTR_ManagedList,
                };
            }
            var query = new QueryExpression()
            {
                EntityName = KTR_ManagedListEntity.EntityLogicalName,
                ColumnSet = new ColumnSet(columns),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression(KTR_ManagedListEntity.Fields.KTR_ManagedList, ConditionOperator.Equal, managedListId),
                        new ConditionExpression(KTR_ManagedListEntity.Fields.StatusCode, ConditionOperator.Equal, (int)KTR_ManagedListEntity_StatusCode.Active)
                    }
                }
            };
            var results = _service.RetrieveMultiple(query);

            return results == null ?
                new List<KTR_ManagedListEntity>() :
                results.Entities
                    .Select(e => e.ToEntity<KTR_ManagedListEntity>())
                    .ToList();
        }
    }
}
