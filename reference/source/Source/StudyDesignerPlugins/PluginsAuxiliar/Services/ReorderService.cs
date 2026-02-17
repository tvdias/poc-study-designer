namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kantar.StudyDesignerLite.PluginsAuxiliar.Helpers;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Messages;

    public class ReorderService
    {
        private readonly ITracingService _tracing;
        private readonly IOrganizationService _service;

        private readonly string _entityLogicalName;
        private readonly string _sortOrderFieldName;

        public ReorderService(
            IOrganizationService service,
            ITracingService tracing,
            string entityLogicalName,
            string sortOrderFieldName)
        {
            _service = service;
            _tracing = tracing;
            _entityLogicalName = entityLogicalName;
            _sortOrderFieldName = sortOrderFieldName;
        }

        public bool ReorderEntities(
            IEnumerable<Guid> ids)
        {
            if (ids == null || !ids.Any())
            {
                _tracing.Trace("No record IDs provided for reordering.");
                return false;
            }

            var orderedRows = ReorderHelper.ToSequentialOrder(ids);

            var success = UpdateBulkEntity(orderedRows);

            if (success)
            {
                _tracing.Trace($"Reordering complete for {_entityLogicalName}.");
            }
            else
            {
                _tracing.Trace($"Error while reordering for {_entityLogicalName}.");
                throw new InvalidPluginExecutionException($"Error while reordering for {_entityLogicalName}.");
            }

            return success;
        }

        #region Queries to Dataverse 
        private bool UpdateBulkEntity(
           IDictionary<Guid, int> orderedRows)
        {
            if (orderedRows == null || orderedRows.Count == 0)
            {
                _tracing.Trace("No rows provided for bulk update.");
                return false;
            }

            var updateRequests = new OrganizationRequestCollection();

            foreach (var row in orderedRows)
            {
                var entity = new Entity(_entityLogicalName, row.Key)
                {
                    [_sortOrderFieldName] = row.Value,
                };

                updateRequests.Add(new UpdateRequest { Target = entity });
            }

            var executeMultiple = new ExecuteMultipleRequest
            {
                Requests = updateRequests,
                Settings = new ExecuteMultipleSettings
                {
                    ContinueOnError = true,
                    ReturnResponses = false
                }
            };

            var response = (ExecuteMultipleResponse)_service.Execute(executeMultiple);

            if (response.IsFaulted)
            {
                _tracing.Trace($"Error while bulk updating for {_entityLogicalName}.");
                return false;
            }
            else
            {
                return true;
            }
        }
        #endregion
    }
}
