namespace Kantar.StudyDesignerLite.PluginsAuxiliar.Repositories.ManagedLists
{
    using System;
    using System.Collections.Generic;
    using Kantar.StudyDesignerLite.Plugins;

    public interface IManagedListEntityRepository
    {
        List<KTR_ManagedListEntity> GetByManagedListId(Guid managedListId, string[] columns = null);
    }
}
