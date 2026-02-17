/**
 * @file        013-project.js
 * @description Set the functionality for Project form.
 *
 * @date        2025-04-01
 * @version     2.0
 *
 * @usage       This script is used to set the AccessTeam tab and custom or Preconfigured toggle visible or not in Project Form.
 * @notes       This script works onLoad and onChange of Project Form and its fields.
 */



var Kantar = Kantar || {};
Kantar.ProjectForm = Kantar.ProjectForm || {};

// Enum of security roles
Kantar.ProjectForm.SecurityRoles = {
    Kantar_BizAdmin: "3e09cf95-954b-f011-877a-6045bd9637eb", // Kantar-BizAdmin
    System_Administrator: "7da62815-9506-f011-bae4-7c1e52277c6c", // System Administrator
    Kantar_Client_Service_User: "c567cc6b-210a-f011-bae2-000d3a2274a5", // Kantar-Client Service User
    Kantar_Scripter: "3a27fcc5-cc0a-f011-bae2-000d3a2274a5", // Kantar - Scripter
    Kantar_Librarian: "72e432d4-1b0a-f011-bae2-6045bd9d7ae1" // Kantar-Librarian
};

// Enum of access team sections
Kantar.ProjectForm.AccessTeamSections = {
    CSScripterUser: "section_AccessTeamCSScripterUser",
    SysAdmin: "section_AccessTeamSysAdmin"
};
Kantar.ProjectForm.TabsToHideOnCreate = {
    QuestionnaireLines: "Questionnaire_tab",
    StudyQuestions: "tab_7",
    ManagedLists: "tab_sharedLists",
    Studies: "tab_5"
}
//=========================================================
//USAGE EXAMPLE:
//Kantar.ProjectForm.onFormLoad(executionContext);
//=========================================================
Kantar.ProjectForm.onFormLoad = async function (executionContext) {
    try {

        // Get the form context from the execution context
        var formContext = executionContext.getFormContext();

        // Retrieve the value of the 'ktr_accessteam' field
        var accessTeamValue = formContext.getAttribute("ktr_accessteam").getValue();
        var customPreconfigured = formContext.getAttribute("ktr_customorpreconfigured").getValue();
        var projectId = formContext.data.entity.getId().replace(/[{}]/g, "");
        if (projectId) {
            var study = await getProjectStudies(projectId);
        
        if (study.length > 0) {
            formContext.getControl("ktr_customorpreconfigured").setDisabled(true);
        }
        else { formContext.getControl("ktr_customorpreconfigured").setDisabled(false); }
        console.log("Access Team Value: ", accessTeamValue);
        }
        // Set the access team tab visibility to the value of the access team indicator
        Kantar.UIGeneral.setTabVisibility(executionContext, 'tabUserManagement', accessTeamValue);
        Kantar.UIGeneral.setTabVisibility(executionContext, 'tab_Product', customPreconfigured);

        // Manage section visibility based on the access team value
        manageSectionVisibility(executionContext);

        //Manage visibility of tabs based on user roles
        hideTabForRole(formContext, "tab_7", Kantar.ProjectForm.SecurityRoles.Kantar_Scripter); //Hide matrix tab for Scripter role
        //Hide tabs in form Create
        Kantar.ProjectForm.hideTabs(executionContext);
    } catch (error) {
        console.error("Error in onFormLoad: ", error.message);
    }
};

Kantar.ProjectForm.onAccessTeamChange = function (executionContext) {
    try {
        // Get the form context from the execution context
        var formContext = executionContext.getFormContext();

        // Retrieve the value of the 'ktr_accessteam' field
        var accessTeamValue = formContext.getAttribute("ktr_accessteam").getValue();

        if (!accessTeamValue) {

            var confirmStrings = {
                text: "By switching off 'Access Team' the team will be deleted. Switching it on afterwards will create a new 'Access Team' and members will need to be added again.\n\nAre you sure you want to proceed?",
                title: "Confirm Action",
                confirmButtonLabel: "Yes",
                cancelButtonLabel: "No"
            };

            var confirmOptions = { height: 300, width: 450 };

            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                function (success) {
                    if (success.confirmed) {
                    } else {
                        // Set the value of the 'ktr_accessteam' field to true
                        formContext.getAttribute("ktr_accessteam").setValue(true);
                        Kantar.UIGeneral.setTabVisibility(executionContext, 'tabUserManagement', true);
                    }
                },
                function (error) {
                    console.log("Error displaying the confirmation dialog: " + error.message);
                }
            );
        }

        // Set the access team tab visibility to the value of the access team indicator
        Kantar.UIGeneral.setTabVisibility(executionContext, 'tabUserManagement', accessTeamValue);
    } catch (error) {
        console.error("Error in onAccessTeamChange: ", error.message);
    }
};

Kantar.ProjectForm.onCustomOrPreconfigureChange = function (executionContext) {
    var formContext = executionContext.getFormContext();

    // Retrieve the value of the 'ktr_customorpreconfigured' field
    var customPreconfigured = formContext.getAttribute("ktr_customorpreconfigured").getValue();
    const projectId = formContext.data.entity.getId().replace(/[{}]/g, "");

    if (projectId) {
        var customOrPreconfigureControl = formContext.getControl("ktr_customorpreconfigured");
        if (customOrPreconfigureControl) {
            var confirmStrings = {
                text: "When switching between 'Custom' and 'Preconfigured' the existing master questionnaire will be cleared of all questions and/or modules.\n\nAre you sure you want to proceed?",
                title: "Confirm Action",
                confirmButtonLabel: "Yes",
                cancelButtonLabel: "No"
            };

            var confirmOptions = { height: 300, width: 450 };

            Xrm.Navigation.openConfirmDialog(confirmStrings, confirmOptions).then(
                async function (success) {
                    if (success.confirmed) {
                        Kantar.UIGeneral.setTabVisibility(executionContext, 'tab_Product', customPreconfigured);
                        try {
                            const deactivated = await deactivateQuestions(projectId);
                            if (deactivated) {
                                // Adding autosave and removing Progress indicator so Ribbon buttons can work appropriately.
                                // Save only if the form has unsaved changes
                                if (formContext.data.getIsDirty()) {
                                    await formContext.data.save();
                                }
                                formContext.ui.refreshRibbon();
                                Xrm.Utility.alertDialog("All questions deactivated successfully.");
                            } else {
                                Xrm.Utility.alertDialog("No active questions found for this project.");
                            }
                        } catch (error) {
                            console.error("Error during question deactivation:", error.message);
                            Xrm.Utility.alertDialog("Something went wrong: " + error.message);
                        }
                    } else {
                        customPreconfigured = !customPreconfigured;
                        formContext.getAttribute("ktr_customorpreconfigured").setValue(customPreconfigured);
                        Kantar.UIGeneral.setTabVisibility(executionContext, 'tab_Product', customPreconfigured);
                    }
                },
                function (error) {
                    console.log("Error displaying the confirmation dialog: " + error.message);
                }
            );
        }
    }
};

Kantar.ProjectForm.SidePanel = function (primaryItemIds) {
    // Side Dialog
    var entityId = primaryItemIds[0].replace(/[{}]/g, "");
    var pageInput = {
        pageType: "entityrecord",
        entityName: "kt_project",
        entityId: entityId,
        formId: "d2a4479c-cd6e-f011-b4cc-7c1e527143d7"

    };
    var navigationOptions = {
        target: 2,
        position: 2,
        width: { value: 500, unit: "px" },
        title: "Create Document"
    };
    Xrm.Navigation.navigateTo(pageInput, navigationOptions)
        .then(
            function () {

            }
        ).catch(
            function (error) {
                console.log("Web resource error");
            }
        );
};

Kantar.ProjectForm.displayButton = function (primaryItemIds) {
    const disallowedRoleNames = ["3a27fcc5-cc0a-f011-bae2-000d3a2274a5"];//Kantar- Scripter
    const userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

    for (let i = 0; i < userRoles.getLength(); i++) {
        const role = userRoles.get(i);
        if (disallowedRoleNames.includes(role.id)) {
            return false; // hide the button
        }
    }
    return true; // show the button
}

async function deactivateQuestions(projectId) {
    try {
        const response = await Xrm.WebApi.online.retrieveMultipleRecords(
            "kt_questionnairelines",
            `?$filter=_ktr_project_value eq ${projectId} and statecode eq 0&$select=kt_name,_ktr_module_value`
        );

        const entities = response.entities;

        if (entities.length === 0) {
            return false;
        }

        const recordIds = entities.map((record) => record["kt_questionnairelinesid"]);

        if (recordIds.length > 0) {
            const conditions = recordIds.map(id => `
                <condition attribute="ktr_questionnaireline" operator="eq" value="${id}" />
            `).join('');

            const fetchXml = `
                <fetch>
                    <entity name="ktr_questionnairelinesanswerlist">
                        <attribute name="ktr_questionnairelinesanswerlistid" />
                        <filter type="or">
                            ${conditions}
                        </filter>
                    </entity>
                </fetch>
            `;

            const encodedFetchXml = encodeURIComponent(fetchXml);

            const answerlistResponse = await Xrm.WebApi.online.retrieveMultipleRecords(
                "ktr_questionnairelinesanswerlist",
                `?fetchXml=${encodedFetchXml}`
            );

            if (answerlistResponse.entities && answerlistResponse.entities.length > 0) {
                await Promise.all(
                    answerlistResponse.entities.map((answer) =>
                        deactivateRecord("ktr_questionnairelinesanswerlist", answer.ktr_questionnairelinesanswerlistid)
                    )
                );
            }
        }

        await Promise.all(
            recordIds.map((recordId) =>
                deactivateRecord("kt_questionnairelines", recordId)
            )
        );

        return true;
    }
    catch (error) {
        console.error("Error retrieving or deactivating questions: ", error.message);
        Xrm.Utility.alertDialog("Error retrieving or deactivating questions: " + error.message);
        return false;
    }
}

async function getProjectStudies(projectId) {

    return Xrm.WebApi.online

        .retrieveMultipleRecords(

            "kt_study",

            `?$filter=_kt_project_value eq ${projectId}&$select=kt_name`

        )

        .then((response) => {

            return response.entities;

        })

        .catch((error) => {

            Xrm.Utility.alertDialog(

                "Error retrieving existing Questions: " + error.message

            );

            return [];

        });

}

async function refreshRibbon(executionContext) {
    const formContext = executionContext.getFormContext();

    try {
        formContext.ui.refreshRibbon();
    }
    catch (error) {
        console.error("Error refreshing ribbon:", error.message);
    }
}

async function deactivateRecord(entityLogicalName, id) {
    try {
        console.log(`Trying to deactivate: ${entityLogicalName}, ID: ${id}`);

        await Xrm.WebApi.online.updateRecord(entityLogicalName, id, {
            statecode: 1,
            statuscode: 2
        });

        console.log(`Deactivated: ${entityLogicalName} (${id})`);
    }
    catch (err) {
        console.error(`Failed to deactivate ${entityLogicalName} ${id}:`, err.message);
        Xrm.Utility.alertDialog(`Failed to deactivate ${entityLogicalName}: ${err.message}`);
    }
}

function manageSectionVisibility(executionContext) {
    var formContext = executionContext.getFormContext();
    var userRoles = getCurrentUserSecurityRoles();

    // Check if the user has the Kantar_BizAdmin or System_Administrator role
    var isAdmin = userRoles.some(role =>
        role.id === Kantar.ProjectForm.SecurityRoles.Kantar_BizAdmin ||
        role.id === Kantar.ProjectForm.SecurityRoles.System_Administrator
    );

    // Check if the user has the Kantar_Client_Service_User or Kantar_Scripter role
    var isClientServiceUser = userRoles.some(role =>
        role.id === Kantar.ProjectForm.SecurityRoles.Kantar_Client_Service_User ||
        role.id === Kantar.ProjectForm.SecurityRoles.Kantar_Scripter ||
        role.id === Kantar.ProjectForm.SecurityRoles.Kantar_Librarian
    );

    var targetTab = formContext.ui.tabs.get("tabUserManagement");

    if (targetTab) {

        // If the user is an admin, show the Sys Admin section
        if (isAdmin) {
            var sysAdminSection = targetTab.sections.get(Kantar.ProjectForm.AccessTeamSections.SysAdmin);
            if (sysAdminSection) {
                sysAdminSection.setVisible(true);
            }
        }

        // If the user is an CS/Scripter, show the CS and Scripter Project Team Form
        if (isClientServiceUser) {
            var csScripterSection = targetTab.sections.get(Kantar.ProjectForm.AccessTeamSections.CSScripterUser);
            if (csScripterSection) {
                csScripterSection.setVisible(true);
            }
        }
    }
}

/**
 * Fetches the current user's security roles using the Xrm.Utility API.
 * @returns {Array} Array of role objects with id and name.
 */
function getCurrentUserSecurityRoles() {
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;
    var roles = [];
    for (var i = 0; i < userRoles.getLength(); i++) {
        var role = userRoles.get(i);
        roles.push({
            id: role.id,
            name: role.name
        });
    }
    return roles;
}

/**
 * Hides a tab if the current user has the specified role
 * @param {object} formContext - The form context
 * @param {string} tabId - The logical name of the tab
 * @param {string} roleId - The GUID of the security role
 */
function hideTabForRole(formContext, tabId, roleId) {
    var userRoles = Xrm.Utility.getGlobalContext().userSettings.roles.getAll();

    var hasRole = userRoles.some(function (role) {
        return role.id.toLowerCase() === roleId.toLowerCase();
    });

    if (hasRole) {
        var tab = formContext.ui.tabs.get(tabId);
        if (tab) {
            tab.setVisible(false);
        }
    }
}
/**
 * Hides a tab if the form is create/new
 * @param {object} executionContext - The executionContext
 */
Kantar.ProjectForm.hideTabs = function (executionContext) {

    var formContext = executionContext.getFormContext();
    var formType = formContext.ui.getFormType();
    //If formtype is Create, hide following tabs on form     
    if (formType == 1) {
        Object.values(Kantar.ProjectForm.TabsToHideOnCreate).forEach(tabName => {
        const tab = formContext.ui.tabs.get(tabName);
        if (tab) {
            tab.setVisible(false);
        }
    });

    }
}  