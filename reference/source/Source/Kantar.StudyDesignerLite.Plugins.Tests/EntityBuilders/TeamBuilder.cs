using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class TeamBuilder
    {
        private readonly Team _entity;

        public TeamBuilder()
        {
            _entity = new Team
            {
                Id = Guid.NewGuid(),
                TeamType = Team_TeamType.SecurityGroup
            };
        }

        public TeamBuilder WithName(string name)
        {
            _entity.Name = name;
            return this;
        }

        public TeamBuilder WithTeamType(Team_TeamType teamType)
        {
            _entity.TeamType = teamType;
            return this;
        }
        public Team Build()
        {
            return _entity;
        }
    }
}
