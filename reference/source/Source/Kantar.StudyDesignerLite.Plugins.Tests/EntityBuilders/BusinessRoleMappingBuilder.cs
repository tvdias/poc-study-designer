using System;
using Microsoft.Xrm.Sdk;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class BusinessRoleMappingBuilder
    {
        private readonly KTR_BusinessRoleMapping _entity;

        public BusinessRoleMappingBuilder()
        {
            _entity = new KTR_BusinessRoleMapping
            {
                Id = Guid.NewGuid(),
                KTR_Name = "Default Business Role Mapping"
            };
        }

        public BusinessRoleMappingBuilder WithCSKantarBusinessRole(Team team)
        {
            _entity.KTR_Team = new EntityReference(team.LogicalName, team.Id);
            _entity.KTR_KAnTarBusinessRole = KTR_KantarBusinessRole.KantarCsUser;
            return this;
        }

        public BusinessRoleMappingBuilder WithKantarScripterRole(Team team)
        {
            _entity.KTR_Team = new EntityReference(team.LogicalName, team.Id);
            _entity.KTR_KAnTarBusinessRole = KTR_KantarBusinessRole.KantarScripter;
            return this;
        }

        public BusinessRoleMappingBuilder WithKantarLibrarianRole(Team team)
        {
            _entity.KTR_Team = new EntityReference(team.LogicalName, team.Id);
            _entity.KTR_KAnTarBusinessRole = KTR_KantarBusinessRole.KantarLibrarian;
            return this;
        }

        public BusinessRoleMappingBuilder WithOtherRole(Team team)
        {
            _entity.KTR_Team = new EntityReference(team.LogicalName, team.Id);
            _entity.KTR_KAnTarBusinessRole = KTR_KantarBusinessRole.Other;
            return this;
        }

        public KTR_BusinessRoleMapping Build()
        {
            return _entity;
        }
    }
}
