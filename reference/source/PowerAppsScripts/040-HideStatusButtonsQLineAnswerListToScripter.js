/**
 * @file        040-HideStatusButtonsQLineAnswerListToScripter.js
 * @description This hides or unhide the Activate / Deactivate / Add Answers buttons for Questionnaire Line answer list record to Scripter
 *
 * @date        2025-08-12
 * @version     1.0
 *
 * @usage       Used on Activate / Deactivate/ Add Answers buttons on grid and form of Questionnaire Line answer list, through ribbon workbench
 * @notes       
 */

var QuestionnaireLinesAnswerListSecurity = (function () {
    "use strict";

    const Scripter = ["3a27fcc5-cc0a-f011-bae2-000d3a2274a5"];

    const control = {
        isDummy: "ktr_isdummyquestion",
       
    };

    class QuestionnaireLinesAnswerListSecurity {
       static  isScripter() {
    const disallowedRoleIds = Scripter; // Kantar - scripter role
    const userRoles = Xrm.Utility.getGlobalContext().userSettings.roles;

    for (let i = 0; i < userRoles.getLength(); i++) {
        const role = userRoles.get(i);
        if (disallowedRoleIds.includes(role.id)) {
            return false; // hide the button
        }
    }
    return true; // show the button
}
static  canEditAnswerList(primaryControl) {
     var formContext = primaryControl;
     var isDummy = formContext.getAttribute(control.isDummy)?.getValue();   
    var scripter = QuestionnaireLinesAnswerListSecurity.isScripter();
    if (!scripter && !isDummy) {
        return false; // show buttons
    }
    return true; // hide buttons
}
  }  return QuestionnaireLinesAnswerListSecurity;
})()