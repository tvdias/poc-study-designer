using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class SystemUserBuilder
    {
        private readonly SystemUser _entity;

        public SystemUserBuilder()
        {
            _entity = new SystemUser
            {
                Id = Guid.NewGuid(),
            };
        }
        public SystemUserBuilder WithKantarScripterRoleProfile()
        {
            _entity[SystemUser.Fields.KTR_BusinessRole] = new OptionSetValue((int)KTR_KantarBusinessRole.KantarScripter);
            _entity.FormattedValues[SystemUser.Fields.KTR_BusinessRole] = "Kantar Scripter";
            return this;
        }

        public SystemUserBuilder WithKantarCSUserRoleProfile()
        {
            _entity[SystemUser.Fields.KTR_BusinessRole] = new OptionSetValue((int)KTR_KantarBusinessRole.KantarCsUser);
            _entity.FormattedValues[SystemUser.Fields.KTR_BusinessRole] = "Kantar CS User";
            return this;
        }

        public SystemUserBuilder WithKantarLibrarianRoleProfile()
        {
            _entity[SystemUser.Fields.KTR_BusinessRole] = new OptionSetValue((int)KTR_KantarBusinessRole.KantarLibrarian);
            _entity.FormattedValues[SystemUser.Fields.KTR_BusinessRole] = "Kantar Librarian";
            return this;
        }

        public SystemUser Build()
        {
            return _entity;
        }
    }
}
