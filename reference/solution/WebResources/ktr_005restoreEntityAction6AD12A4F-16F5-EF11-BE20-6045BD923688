/**
 * @file        restoreEntityAction.js
 * @description This simply restores Entity by changing State = ACTIVE and Status = DRAFT
 *              
 * @date        2025-02-18
 * @version     2.0
 * 
 * @usage       This script is being called in Module/QuestionBank form in 'Restore' button (when Module/QuestionBank is INACTIVE)
 * @notes       This script uses Xrm Power Apps library
*/
var RestoreEntityAction = (function(config) {
    const stateActive = config.state.active;
    const statusDraft = config.status.draft;

    function restoreEntity(primaryControl) {
        if (!primaryControl) return;
    
        var formContext = primaryControl;
    
        var entity = {};
        entity["statecode"] = stateActive;
        entity["statuscode"] = statusDraft;
    
        Xrm.WebApi.updateRecord(formContext.data.entity.getEntityName(), formContext.data.entity.getId(), entity)
            .then(function success(result) {
                formContext.data.refresh(); // Refresh the form after update
            },
            function error(error) {
                console.log("Error updating record: " + error.message);
            });
    }

    return {
        restoreEntity: restoreEntity,
    };
})({
    state: {
        active: 0,
        inactive: 1
    },
    status: {
        active: 1,
        inactive: 2,
        draft: 847610001,
    }
});