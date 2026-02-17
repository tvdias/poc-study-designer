using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ProjectBuilder
    {
        private readonly KT_Project _entity;

        public ProjectBuilder() 
        { 
            _entity = new KT_Project
            {
                Id = Guid.NewGuid(),
                StateCode = KT_Project_StateCode.Active,
                StatusCode = KT_Project_StatusCode.Active,
            };
        }

        public ProjectBuilder WithName(string name)
        {
            _entity.KT_Name = name;
            return this;
        }

        public ProjectBuilder WithProductTemplate(KTR_ProductTemplate productTemplate)
        {
            _entity.KTR_ProductTemplate = new EntityReference(productTemplate.LogicalName, productTemplate.Id);
            return this;
        }

        public ProjectBuilder WithProduct(KTR_Product product)
        {
            _entity.KTR_Product = new EntityReference(product.LogicalName, product.Id);
            return this;
        }

        public ProjectBuilder WithTeamAccess(Team team)
        {
            _entity.KTR_TeamAccess = new EntityReference(Team.EntityLogicalName, team.Id);
            _entity.KTR_AccessTeam = true;
            return this;
        }

        public ProjectBuilder WithOwner(Guid ownerId)
        {
            _entity.KTR_TeamAccess = new EntityReference(Owner.EntityLogicalName, ownerId);
            return this;
        }
        public ProjectBuilder WithAccessTeam(bool value)
        {
            _entity.KTR_AccessTeam = value;
            return this;
        }

        public KT_Project Build()
        {
            return _entity;
        }
    }
}
