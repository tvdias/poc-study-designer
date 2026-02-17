namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Models.QuestionnaireLine.QuestionnaireLineAddQuestionsOrModules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class AddQuestionsOrModulesRequest
    {
        public Guid ProjectId { get; set; }
        public int SortOrder { get; set; }
        public EntityTypeEnum EntityType { get; set; }
        public IEnumerable<RowEntityRequest> Rows { get; set; }
    }
}
