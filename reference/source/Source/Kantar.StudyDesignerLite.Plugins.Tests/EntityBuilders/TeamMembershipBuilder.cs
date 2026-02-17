using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class TeamMembershipBuilder
    {
        private readonly TeamMembership _entity;

        public TeamMembershipBuilder()
        {
            _entity = new TeamMembership
            {
                Id = Guid.NewGuid(),
            }; 
        }

        public TeamMembershipBuilder WithTeamMember(Team team, SystemUser user)
        {
            _entity[TeamMembership.Fields.TeamId] = team.Id;
            _entity[TeamMembership.Fields.SystemUserId] = user.Id;
            return this;
        }

        public TeamMembership Build()
        {
            return _entity;
        }
    }
}
