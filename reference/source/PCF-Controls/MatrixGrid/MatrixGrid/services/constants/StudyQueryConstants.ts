export const StudyQueryConstants = {
    GET_STUDIES_BY_MANAGED_LIST: `
        <fetch top="100">
            <entity name="kt_study">
                <attribute name="kt_studyid" alias="id" />
                <attribute name="kt_name" alias="name" />
                <attribute name="ktr_versionnumber" alias="version" />
                <attribute name="ktr_masterstudy" alias="masterid" />
                <attribute name="statuscode" />
                <order attribute="createdon" descending="true" />
                <filter type="and">
                    <condition attribute="statuscode" operator="neq" value="847610005" />
                    <condition attribute="statecode" operator="eq" value="0" />
                </filter>
                <link-entity name="ktr_managedlist" from="ktr_project" to="kt_project" link-type="inner">
                    <filter type="and">
                        <condition attribute="ktr_managedlistid" operator="eq" value="{MANAGED_LIST_ID}" />
                    </filter>
                </link-entity>
            </entity>
        </fetch>`
};