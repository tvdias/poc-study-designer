namespace DigTx.Designer.FunctionApp.Infrastructure.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kantar.StudyDesignerLite.Plugins;

public interface IModuleRepository : IBaseRepository<KT_Module>
{
    /// <summary>
    /// Get Module with its related Question Banks.
    /// </summary>
    /// <param name="id"> Module Id.</param>
    /// <returns> Return a list of Modules.</returns>
    Task<IList<KT_Module>> GetWithQuestionAsync(Guid id);
}
