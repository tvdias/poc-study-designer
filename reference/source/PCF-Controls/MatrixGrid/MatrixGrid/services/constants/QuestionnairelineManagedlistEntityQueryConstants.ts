export const QuestionnairelineManagedlistEntityQueryConstants = {
    GET_QUESTIONNAIRELINEMANAGEDLISTENTITIES_BY_STUDY_MANAGEDLIST: `
        <fetch>
          <entity name="ktr_questionnairelinemanagedlistentity">
            <attribute name="ktr_questionnairelinemanagedlistentityid" alias="id" />
            <filter>
              <condition attribute="ktr_studyid" operator="eq" value="{STUDY_ID}" />
              <condition attribute="ktr_managedlist" operator="eq" value="{MANAGED_LIST_ID}" />
              <condition attribute="statecode" operator="eq" value="0" />
            </filter>
          </entity>
        </fetch> `,
     GET_QUESTIONNAIRELINEMANAGEDLISTENTITIES_BY_MANAGEDLIST: `
        <fetch>
          <entity name="ktr_questionnairelinemanagedlistentity">
            <attribute name="ktr_questionnairelinemanagedlistentityid" alias="id" />
            <attribute name="ktr_managedlistentity" alias="rowId" />
            <attribute name="ktr_questionnaireline" alias="columnId" />
            <attribute name="ktr_studyid" alias="dropdownValueToFilter" />
            <filter type="and">
              <condition attribute="statecode" operator="eq" value="0" />
            </filter>
            <link-entity name="ktr_managedlist" from="ktr_managedlistid" to="ktr_managedlist" link-type="inner">
              <filter type="and">
                <condition attribute="ktr_managedlistid" operator="eq" value="{MANAGED_LIST_ID}" />
              </filter>
            </link-entity>
          </entity>
        </fetch>
 `, 
};