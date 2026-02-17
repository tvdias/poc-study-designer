using Kantar.StudyDesignerLite.PluginsAuxiliar.Services.General;
using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Builders
{
    public class UrlBuilder
    {
        private readonly IEnvironmentVariablesService _environmentService;
        private const string EnvVariableNameOrgUrl = "ktr_OrgUrl";
        private const string EnvVariableNameAppId = "ktr_AppId";

        private string _orgUrl;
        private string _appId;
        private string _entityLogicalName;
        private Guid _entityId;

        public UrlBuilder(IEnvironmentVariablesService environmentService)
        {
            _environmentService = environmentService
                ?? throw new ArgumentNullException(nameof(environmentService));
        }

        public UrlBuilder WithOrgUrl()
        {
            _orgUrl = _environmentService
                .GetEnvironmentVariableValue(EnvVariableNameOrgUrl)?
                .TrimEnd('/');

            return this;
        }

        public UrlBuilder WithAppId()
        {
            _appId = _environmentService
                .GetEnvironmentVariableValue(EnvVariableNameAppId);

            return this;
        }

        public UrlBuilder WithEntity(string entityLogicalName)
        {
            _entityLogicalName = entityLogicalName;
            return this;
        }

        public UrlBuilder WithId(Guid entityId)
        {
            _entityId = entityId;
            return this;
        }

        public string BuildEntityUrl()
        {
            if (string.IsNullOrEmpty(_orgUrl))
            {
                throw new InvalidOperationException("Organization URL must be provided.");
            }

            if (string.IsNullOrEmpty(_appId))
            {
                throw new InvalidOperationException("App ID must be provided.");
            }

            if (string.IsNullOrEmpty(_entityLogicalName))
            {
                throw new InvalidOperationException("Entity logical name must be provided.");
            }

            if (_entityId == Guid.Empty)
            {
                throw new InvalidOperationException("Entity Id must be provided.");
            }

            return $"{_orgUrl}/main.aspx?appid={_appId}&pagetype=entityrecord&etn={_entityLogicalName}&id={_entityId}";
        }
    }
}
