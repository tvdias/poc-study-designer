/**
 * @file        041-filterImpactedQuestion.js
 * @description Set to Filter ImpactedQuestion lookup  and show questions from the config question which are in Product.
 *
 * @date        2025-08-23
 * @version     1.0
 *
 * @usage       This script is invoked on load of the Main of Product Config Question Display Rule.
 * @notes       This script uses Xrm Power Apps library
 */

var ConfigQuestionFilter = (function () {
    "use strict";

    const entity = {
        productConfigQuestion: "ktr_productconfigquestion",
        impactedConfigQuestion: "ktr_impactedconfigquestion",
        configQuestion: "ktr_configurationquestion"
    };

    const control = {
        productConfigQuestion: "ktr_productconfigquestion",
        impactedConfigQuestion: "ktr_impactedconfigquestion"
    };

    class ConfigQuestionFilter {
        static async onLoad(executionContext) {
            try {
                var formContext = executionContext.getFormContext();
                var impactedControl = formContext.getControl(control.impactedConfigQuestion);

                if (impactedControl) {
                    impactedControl.addPreSearch(async () => {
                        await ConfigQuestionFilter.filterImpactedQuestions(executionContext);
                    });

                }
// Preload the filter so lookup works on first click
            ConfigQuestionFilter.filterImpactedQuestions(executionContext)
                .then(() => {
                    // Force refresh so the lookup applies the filter immediately
                    impactedControl.refresh();
                })
                .catch(error => console.error("Error preloading filter:", error));
        }
             catch (error) {
                console.error("Error in onLoad:", error);
            }
        }

        static async filterImpactedQuestions(executionContext) {
            try {
                var formContext = executionContext.getFormContext();
                var pcq = formContext.getAttribute(control.productConfigQuestion)?.getValue();
                if (!pcq || pcq.length === 0) return;

                var pcqId = pcq[0].id.replace(/[{}]/g, "");

                // ✅ Step 1: Get Product from selected ProductConfigQuestion
                var pcqRecord = await Xrm.WebApi.retrieveRecord(entity.productConfigQuestion, pcqId, "?$select=_ktr_product_value");
                if (!pcqRecord._ktr_product_value) return;

                var productId = pcqRecord._ktr_product_value;

                // ✅ Step 2: Get all ProductConfigQuestions for this Product
                var res = await Xrm.WebApi.retrieveMultipleRecords(entity.productConfigQuestion,
                    `?$filter=_ktr_product_value eq ${productId} and statecode eq 0&$select=_ktr_configurationquestion_value`
                );

                if (res.entities.length === 0) return;

                // ✅ Step 3: Collect ConfigQuestion IDs
                var ids = [];
                res.entities.forEach(pcqRow => {
                    if (pcqRow._ktr_configurationquestion_value) {
                        ids.push(pcqRow._ktr_configurationquestion_value.replace(/[{}]/g, ""));
                    }
                });
                if (ids.length === 0) return;

                // ✅ Step 4: Build fetchXml with IN condition
                let inConditions = "";
                ids.forEach(id => {
                    inConditions += `<value>${id}</value>`;
                });

               var fetchXml = `
    <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="true">
      <entity name="${entity.configQuestion}">
        <attribute name="ktr_configurationquestionid" />
        <attribute name="ktr_name" />
        <filter type="and">
          <condition attribute="statecode" operator="eq" value="0" /> <!-- active only -->
          <condition attribute="ktr_configurationquestionid" operator="in">
            ${inConditions}
          </condition>
        </filter>
      </entity>
    </fetch>`;

                // ✅ Step 5: Define custom view
                var layoutXml = `
                    <grid name="resultset" object="1" jump="ktr_name" select="1" icon="1" preview="1">
                        <row name="result" id="ktr_configurationquestionid">
                            <cell name="ktr_name" width="300" />
                        </row>
                    </grid>`;

                var viewId = "{B1234567-89AB-4CDE-F012-3456789ABCDE}"; // random GUID for custom view
                var viewDisplayName = "Filtered Config Questions";

                // ✅ Step 6: Apply custom view to lookup
                var impactedControl = formContext.getControl(control.impactedConfigQuestion);
                if (impactedControl) {
                    impactedControl.addCustomView(viewId, entity.configQuestion, viewDisplayName, fetchXml, layoutXml, true);
                }
            } catch (error) {
                console.error("Error filtering impacted config questions:", error);
            }
        }
    }

    return ConfigQuestionFilter;
})();
