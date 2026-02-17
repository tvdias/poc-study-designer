/**
 * @file        012-setLatestProductTemplate.js
 * @description Set product template of latest version, when product is selected on a project
 *
 * @date        2025-03-24
 * @version     1.0
 *
 * @usage       This script is invoked on change of product field on project MDA form.
 * @notes       This script uses Xrm Power Apps library
 */


function setLatestProductTemplate(executionContext) {
    var formContext = executionContext.getFormContext();

    // Get selected Product ID
    var productField = formContext.getAttribute("ktr_product");
    if (!productField || !productField.getValue()) {
        return; // Exit if no product is selected
    }
    var productId = productField.getValue()[0].id.replace(/[{}]/g, ""); // Remove curly braces

    // Fetch related Product Templates sorted by Version Descending
    var fetchXml = `
        <fetch top="1">
            <entity name="ktr_producttemplate">
                <attribute name="ktr_producttemplateid" /> <!-- Primary Key -->
                <attribute name="ktr_name" /> <!-- Name field -->
                <attribute name="ktr_version" />
                <attribute name="statuscode"/>
                <filter>
                    <condition attribute="ktr_product" operator="eq" value="${productId}" />
                    <condition attribute="statuscode" operator="eq" value="1"/>
                </filter>
                <order attribute="ktr_version" descending="true" />
            </entity>
        </fetch>`;

    Xrm.WebApi.retrieveMultipleRecords("ktr_producttemplate", "?fetchXml=" + encodeURIComponent(fetchXml))
        .then(function (result) {
            if (result.entities.length > 0) {
                var latestTemplate = result.entities[0];

                // Set the latest Product Template in the lookup field
                formContext.getAttribute("ktr_producttemplate").setValue([
                    {
                        id: latestTemplate.ktr_producttemplateid,
                        entityType: "ktr_producttemplate",
                        name: latestTemplate.ktr_name
                    }
                ]);
                formContext.getAttribute("ktr_producttemplate").fireOnChange();

            }
        })
        .catch(function (error) {
            console.error("Error fetching Product Templates: ", error);
        });
}
