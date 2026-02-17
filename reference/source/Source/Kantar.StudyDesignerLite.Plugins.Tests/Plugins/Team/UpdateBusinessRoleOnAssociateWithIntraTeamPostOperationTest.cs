using System.Web.Services.Description;
using FakeXrmEasy;
using Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders;
using Kantar.StudyDesignerLite.Plugins.User;
using Kantar.StudyDesignerLite.PluginsAuxiliar.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Moq;

namespace Kantar.StudyDesignerLite.Plugins.Tests.Plugins.Team
{
    [TestClass]
    public class UpdateBusinessRoleOnAssociateWithIntraTeamPostOperationTest
    {
        private XrmFakedContext _context;
        private IOrganizationService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _context = new XrmFakedContext();
            _service = _context.GetOrganizationService();
        }

        [TestMethod]
        public void ExecuteCdsPlugin_UpdateBusinessRoleForCSUser()
        {
            // Arrange Team
            var teamEntity = new TeamBuilder()
                .WithName("SDLite_ClientService")
                .Build();

            // Arrange Business Role Mapping
            var businessRoleMapping = new BusinessRoleMappingBuilder()
                .WithCSKantarBusinessRole(teamEntity)
                .Build();

            // Arrange System User
            var systemUserEntity = new SystemUserBuilder()
                .WithKantarLibrarianRoleProfile()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Associate);
            pluginContext.InputParameters["Target"] = new EntityReference(teamEntity.LogicalName, teamEntity.Id);
            // Set the relationship for team membership association
            pluginContext.InputParameters["Relationship"] = new Relationship("teammembership_association");
            EntityReference relatedentity = new EntityReference(systemUserEntity.LogicalName, systemUserEntity.Id);
            EntityReferenceCollection relatedEntities = new EntityReferenceCollection
            {
                relatedentity
            };
            relatedEntities.Add(relatedentity);
            // Set the related entities collection
            pluginContext.InputParameters["RelatedEntities"] = relatedEntities;

            // Initialize the context with the team and system user entities
            _context.Initialize(new Entity[] { teamEntity, systemUserEntity, businessRoleMapping });

            // Act
            var label = _context.ExecutePluginWith<UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation>(pluginContext);

            // Assert

            var updatedUser = _service.Retrieve(SystemUser.EntityLogicalName, systemUserEntity.Id, new ColumnSet(SystemUser.Fields.KTR_BusinessRole)).ToEntity<SystemUser>();
            var businessRole = updatedUser.GetAttributeValue<OptionSetValue>(SystemUser.Fields.KTR_BusinessRole);

            Assert.AreEqual(KTR_KantarBusinessRole.KantarCsUser, updatedUser.KTR_BusinessRole);
        }

        [TestMethod]
        public void ExecuteCdsPlugin_UpdateBusinessRoleForScripterUser()
        {
            // Arrange Team
            var teamEntity = new TeamBuilder()
                .WithName("SDLite_Scripter")
                .Build();

            // Arrange Business Role Mapping
            var businessRoleMapping = new BusinessRoleMappingBuilder()
                .WithKantarScripterRole(teamEntity)
                .Build();

            // Arrange System User
            var systemUserEntity = new SystemUserBuilder()
                .WithKantarCSUserRoleProfile()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Associate);
            pluginContext.InputParameters["Target"] = new EntityReference(teamEntity.LogicalName, teamEntity.Id);
            // Set the relationship for team membership association
            pluginContext.InputParameters["Relationship"] = new Relationship("teammembership_association");
            EntityReference relatedentity = new EntityReference(systemUserEntity.LogicalName, systemUserEntity.Id);
            EntityReferenceCollection relatedEntities = new EntityReferenceCollection
            {
                relatedentity
            };

            relatedEntities.Add(relatedentity);
            // Set the related entities collection
            pluginContext.InputParameters["RelatedEntities"] = relatedEntities;

            // Initialize the context with the team and system user entities
            _context.Initialize(new Entity[] { teamEntity, systemUserEntity, businessRoleMapping });

            // Act
            var label = _context.ExecutePluginWith<UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation>(pluginContext);

            // Assert
            var updatedUser = _service.Retrieve(SystemUser.EntityLogicalName, systemUserEntity.Id, new ColumnSet(SystemUser.Fields.KTR_BusinessRole)).ToEntity<SystemUser>();
            var businessRole = updatedUser.GetAttributeValue<OptionSetValue>(SystemUser.Fields.KTR_BusinessRole);

            Assert.AreEqual(KTR_KantarBusinessRole.KantarScripter, updatedUser.KTR_BusinessRole);
        }

        [TestMethod]
        public void ExecuteCdsPlugin_UpdateBusinessRoleForLibrarianUser()
        {
            // Arrange Team
            var teamEntity = new TeamBuilder()
                .WithName("SDLite_Librarian")
                .Build();

            // Arrange Business Role Mapping
            var businessRoleMapping = new BusinessRoleMappingBuilder()
                .WithKantarLibrarianRole(teamEntity)
                .Build();

            // Arrange System User
            var systemUserEntity = new SystemUserBuilder()
                .WithKantarScripterRoleProfile()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Associate);
            pluginContext.InputParameters["Target"] = new EntityReference(teamEntity.LogicalName, teamEntity.Id);
            // Set the relationship for team membership association
            pluginContext.InputParameters["Relationship"] = new Relationship("teammembership_association");
            EntityReference relatedentity = new EntityReference(systemUserEntity.LogicalName, systemUserEntity.Id);
            EntityReferenceCollection relatedEntities = new EntityReferenceCollection
            {
                relatedentity
            };
            relatedEntities.Add(relatedentity);
            // Set the related entities collection
            pluginContext.InputParameters["RelatedEntities"] = relatedEntities;

            // Initialize the context with the team and system user entities
            _context.Initialize(new Entity[] { teamEntity, systemUserEntity, businessRoleMapping });

            // Act
            var label = _context.ExecutePluginWith<UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation>(pluginContext);

            // Assert
            var updatedUser = _service.Retrieve(SystemUser.EntityLogicalName, systemUserEntity.Id, new ColumnSet(SystemUser.Fields.KTR_BusinessRole)).ToEntity<SystemUser>(); ;
            var businessRole = updatedUser.GetAttributeValue<OptionSetValue>(SystemUser.Fields.KTR_BusinessRole);

            Assert.AreEqual(KTR_KantarBusinessRole.KantarLibrarian, updatedUser.KTR_BusinessRole);
        }

        [TestMethod]
        public void ExecuteCdsPlugin_UpdateNewUserBusinessRoleForOtherUser()
        {
            // Arrange Team
            var teamEntity = new TeamBuilder()
                .WithName("SDLite_UC1SDDEV")
                .Build();

            // Arrange Business Role Mapping
            var businessRoleMapping = new BusinessRoleMappingBuilder()
                .WithOtherRole(teamEntity)
                .Build();

            // Arrange System User
            var systemUserEntity = new SystemUserBuilder()
                .Build();

            var pluginContext = _context.GetDefaultPluginContext();
            pluginContext.MessageName = nameof(ContextMessageEnum.Associate);
            pluginContext.InputParameters["Target"] = new EntityReference(teamEntity.LogicalName, teamEntity.Id);
            // Set the relationship for team membership association
            pluginContext.InputParameters["Relationship"] = new Relationship("teammembership_association");
            EntityReference relatedentity = new EntityReference(systemUserEntity.LogicalName, systemUserEntity.Id);
            EntityReferenceCollection relatedEntities = new EntityReferenceCollection
            {
                relatedentity
            };
            relatedEntities.Add(relatedentity);
            // Set the related entities collection
            pluginContext.InputParameters["RelatedEntities"] = relatedEntities;

            // Initialize the context with the team and system user entities
            _context.Initialize(new Entity[] { teamEntity, systemUserEntity, businessRoleMapping });

            // Act
            var label = _context.ExecutePluginWith<UpdateBusinessRoleOnAssociateWithIntraTeamPostOperation>(pluginContext);

            // Assert
            var updatedUser = _service.Retrieve(SystemUser.EntityLogicalName, systemUserEntity.Id, new ColumnSet(SystemUser.Fields.KTR_BusinessRole)).ToEntity<SystemUser>(); ;
            var businessRole = updatedUser.GetAttributeValue<OptionSetValue>(SystemUser.Fields.KTR_BusinessRole);

            Assert.AreEqual(KTR_KantarBusinessRole.Other, updatedUser.KTR_BusinessRole);
        }
    }
}
