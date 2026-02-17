namespace DigTx.Designer.FunctionApp.Infrastructure.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Plugins;

public interface IQuestionBankRepository : IBaseRepository<KT_QuestionBank>
{
    /// <summary>
    /// Get Question Banks by a list of Ids and ensure they are active.
    /// </summary>
    /// <param name="ids">List of questions Id.</param>
    /// <returns> Returna a list of active question bank.</returns>
    Task<IList<KT_QuestionBank>> GetByIdsAsync(IList<Guid> ids);
}
