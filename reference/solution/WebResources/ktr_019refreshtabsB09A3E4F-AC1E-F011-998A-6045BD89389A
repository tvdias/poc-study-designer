/**
 * @file        019-refreshTabs.js
 * @description Refreshes the drag and drop subgrid present in the Tab
 * @date        2025-04-20
 * @version     1.0
 * 
 * @usage       It runs on the opening of the Tab and refresh the grid control present in the Tab
 * @notes       
 */


var Kantar = Kantar || {};
Kantar.Refresh = Kantar.Refresh || {};

Kantar.Refresh.refreshTabs = function (executionContext) {
   
            const formContext = executionContext.getFormContext();
            const tab = executionContext.getEventSource(); // Tab that triggered the event
        
            if (tab.getDisplayState() === "expanded") {
                
                tab.sections.forEach(function (section) {
                    section.controls.forEach(function (control) {
                        if (control.getControlType()==="customsubgrid:Sherlock.Maverick.Controls.FlexibleOrderingGrid" ) {
                            control.refresh();
                                 } 
                    });
                });
            }
            }
   