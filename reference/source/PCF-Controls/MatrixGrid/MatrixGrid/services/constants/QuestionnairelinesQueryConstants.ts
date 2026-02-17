export const QuestionnairelinesQueryConstants = {
    GET_QUESTIONNAIRELINES_BY_STUDY_MANAGEDLIST: `
        <fetch distinct="true">
            <entity name="ktr_studyquestionnaireline">
                <attribute name="ktr_questionnaireline" alias="id" />
                <attribute name="ktr_study" alias="dropdownValueToFilter" />
                <filter type="and">
                    <condition attribute="statecode" operator="eq" value="0" />
                    <condition attribute="ktr_study" operator="not-null" />
                </filter>
                <link-entity name="ktr_questionnairelinesharedlist" from="ktr_questionnaireline" to="ktr_questionnaireline" link-type="inner">
                    <filter type="and">
                        <condition attribute="statecode" operator="eq" value="0" />
                        <condition attribute="ktr_managedlist" operator="eq" value="{MANAGED_LIST_ID}" />
                    </filter>
                </link-entity>
                <link-entity name="kt_questionnairelines" from="kt_questionnairelinesid" to="ktr_questionnaireline" link-type="inner">
                <attribute name="kt_questionvariablename" alias="name" />
                <filter type="and">
                    <condition attribute="statecode" operator="eq" value="0" />
                </filter>
                </link-entity>
            </entity>
        </fetch> `
};