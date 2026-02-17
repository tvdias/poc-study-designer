namespace DigTx.Designer.FunctionApp.Infrastructure.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Plugins;

public interface IQuestionAnswerListRepository : IBaseRepository<KTR_QuestionAnswerList>
{
    Task<IList<KTR_QuestionAnswerList>> GetByQuestionIdsAsync(IList<Guid> questionIds);
}
