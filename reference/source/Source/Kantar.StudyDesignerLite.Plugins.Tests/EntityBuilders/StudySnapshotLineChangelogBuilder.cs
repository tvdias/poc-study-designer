using System;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class StudySnapshotLineChangelogBuilder
    {
        private readonly Entity _entity;

        public StudySnapshotLineChangelogBuilder(KT_Study study)
        {
            _entity = new Entity(KTR_StudySnapshotLineChangelog.EntityLogicalName)
            {
                Id = Guid.NewGuid()
            };

            if (study != null)
            {
                _entity[KTR_StudySnapshotLineChangelog.Fields.KTR_CurrentStudy] = study.ToEntityReference();
            }
        }

        public StudySnapshotLineChangelogBuilder WithName(string name)
        {
            _entity[KTR_StudySnapshotLineChangelog.Fields.KTR_Name] = name;
            return this;
        }

        public Entity Build()
        {
            return _entity;
        }
    }
}
