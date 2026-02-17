using Microsoft.Xrm.Sdk;
using System;

namespace Kantar.StudyDesignerLite.Plugins.Tests.EntityBuilders
{
    public class ScriptletBuilder
    {
        private readonly KTR_Scriptlets _entity;

        public ScriptletBuilder()
        {
            _entity = new KTR_Scriptlets
            {
                Id = Guid.NewGuid(),
            };
        }

        public ScriptletBuilder WithScriptletInput(string input)
        {
            _entity[KTR_Scriptlets.Fields.KTR_ScriptLetsInput] = input;
            return this;
        }

        public KTR_Scriptlets Build()
        {
            return _entity;
        }
    }
}
