using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProjectProductConfigBuilder
    {
        private readonly KTR_ProjectProductConfig _entity;

        public ProjectProductConfigBuilder(
            KTR_ConfigurationQuestion configQuestion,
            KT_Project project)
        {
            _entity = new KTR_ProjectProductConfig
            {
                Id = Guid.NewGuid(),
                KTR_Name = configQuestion.Id.ToString(),
                KTR_ConfigurationQuestion = new EntityReference(configQuestion.LogicalName, configQuestion.Id),
                KTR_KT_Project = new EntityReference(project.LogicalName, project.Id),
                StateCode = KTR_ProjectProductConfig_StateCode.Active,
                StatusCode = KTR_ProjectProductConfig_StatusCode.Active,
            };
        }

        public KTR_ProjectProductConfig Build()
        {
            return _entity;
        }
    }
}
