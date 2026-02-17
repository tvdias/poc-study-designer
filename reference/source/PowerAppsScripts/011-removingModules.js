/**
 * @file        011-removingModules.js
 * @description Invoke Remove Modules Custom Page
 *
 * @date        2025-03-19
 * @version     1.0
 *
 * @usage       This script is used to invoke Custom Page to remove modules to the Project.
 * @notes        This script works on Remove Module button on Questionnaire Line entity
 */


// Side Dialog
function openRemoveModuleCustomPage(primaryItemIds,SelectedControl){
    var pageInput = {
        pageType: "custom",
        name: "ktr_removemodules_ef6f7",
    recordId: primaryItemIds._entityReference.id.guid
    
    };
    var navigationOptions = {
        target: 2, 
        position: 2,
        width: {value: 500, unit: "px"},
        title: "Remove Module"
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions)
        .then(
            function () {
              SelectedControl.refresh()
            }
        ).catch(
            function (error) {
               console.log("Web resource error");
            }
        );}