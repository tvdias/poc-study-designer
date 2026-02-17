/**
* @file        037-QuestionnaireLineManagedList.js
* @description Will populate project field on Questionnaire line Managed list.
*
* @date        2025-07-30
* @version     1.0
*
* @usage       This script is invoked on load of create of Questionnaire line Managed list.
* @notes       
*/
function setProjectLookup(executionContext) {
    var formContext = executionContext.getFormContext();

    // Check if Project is already populated
    var projectAttribute = formContext.getAttribute("ktr_projectid");
    if (projectAttribute && projectAttribute.getValue()) {
        return;
    }

    // Using the correct field names from your schema
    var questionLineAttribute = formContext.getAttribute("ktr_questionnaireline");
    var managedListAttribute = formContext.getAttribute("ktr_managedlist");

    // Try to get project from Questionnaire Line first
    if (questionLineAttribute && questionLineAttribute.getValue()) {
        var questionLineId = questionLineAttribute.getValue()[0].id.replace(/[{}]/g, "");

        Xrm.WebApi.retrieveRecord("kt_questionnairelines", questionLineId, "?$select=_ktr_project_value").then(
            function success(result) {
                if (result._ktr_project_value) {
                    setProjectValue(formContext, result._ktr_project_value);
                } else {
                }
            }
        );
    }
    // If no questionnaire line, try to get project from managed List
    else if (managedListAttribute && managedListAttribute.getValue()) {
        var managedListId = managedListAttribute.getValue()[0].id.replace(/[{}]/g, "");

        Xrm.WebApi.retrieveRecord("ktr_managedlist", managedListId, "?$select=_ktr_project_value").then(
            function success(result) {
                if (result._ktr_project_value) {
                    setProjectValue(formContext, result._ktr_project_value);
                } else {
                }
            },
            function error(err) {
                Xrm.WebApi.retrieveRecord("ktr_managedlist", managedListId, "?$select=_ktr_project_value").then(
                    function success(result) {
                        if (result._ktr_project_value) {
                            setProjectValue(formContext, result._ktr_project_value);
                        }
                    }
                );
            }
        );
    } else {
    }
}

function setProjectValue(formContext, projectId) {

    // Using correct project entity name: kt_project
    Xrm.WebApi.retrieveRecord("kt_project", projectId, "?$select=kt_name").then(
        function success(projectResult) {

            var projectAttribute = formContext.getAttribute("ktr_projectid");
            if (projectAttribute) {
                projectAttribute.setValue([{
                    id: projectId,
                    name: projectResult.kt_name || "Project",
                    entityType: "kt_project"
                }]);

                // Make field read-only after auto-population
                var projectControl = formContext.getControl("ktr_projectid");
                if (projectControl) {
                    projectControl.setDisabled(true);
                }

                // Unlock Questionnaire Line 
                var questionnaireLineControl = formContext.getControl("ktr_questionnaireline");
                if (questionnaireLineControl) {
                    questionnaireLineControl.setDisabled(false);
            }

                // Unlock Managed List
                var managedListControl = formContext.getControl("ktr_managedlist");
                if (managedListControl) {
                    managedListControl.setDisabled(false);
                }
            }
        },
        function error(err) {

            // If kt_name doesn't exist, try other possible name fields
            Xrm.WebApi.retrieveRecord("kt_project", projectId, "?$select=name").then(
                function success(projectResult) {
                    var projectAttribute = formContext.getAttribute("ktr_projectid");
                    if (projectAttribute) {
                        projectAttribute.setValue([{
                            id: projectId,
                            name: projectResult.name || "Project",
                            entityType: "kt_project"
                        }]);
                    }
                }
            );
        }
    );
}

function onFormLoad(executionContext) {
    var formContext = executionContext.getFormContext();

    // Only run for new records (form type 1)
    if (formContext.ui.getFormType() === 1) {

        //Lock questionnaire line until project is populated
        var questionnaireLineControl = formContext.getControl("ktr_questionnaireline");
        if (questionnaireLineControl) {
            questionnaireLineControl.setDisabled(true);
        }
        
        //Lock managed list until project is populated
        var managedListControl = formContext.getControl("ktr_managedlist");
        if (managedListControl) {
            managedListControl.setDisabled(true);
        }

        // Add a delay to ensure all fields are loaded
        setTimeout(function () {
            setProjectLookup(executionContext);
        }, 500);

        // Also add change events to handle dynamic updates
        var questionLineAttribute = formContext.getAttribute("ktr_questionnaireline");
        var managedListAttribute = formContext.getAttribute("ktr_managedlist");

        if (questionLineAttribute) {
            questionLineAttribute.addOnChange(function (context) {
                setTimeout(function () { setProjectLookup(context); }, 100);
            });
        }

        if (managedListAttribute) {
            managedListAttribute.addOnChange(function (context) {
                setTimeout(function () { setProjectLookup(context); }, 100);
            });
        }
    } else {
    }
}