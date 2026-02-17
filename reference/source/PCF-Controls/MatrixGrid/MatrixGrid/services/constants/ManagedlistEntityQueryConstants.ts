export const ManagedlistEntityQueryConstants = {
    GET_MANAGEDLISTENTITIES_BY_STUDY_MANAGEDLIST: `
        <fetch distinct="true">
            <entity name="ktr_studymanagedlistentity">
                <attribute name="ktr_managedlistentity" alias="id" />
                <attribute name="ktr_study" alias="dropdownValueToFilter" />
                <filter type="and">
                    <condition attribute="statecode" operator="eq" value="0" />
                </filter>
                <link-entity name="ktr_managedlistentity" from="ktr_managedlistentityid" to="ktr_managedlistentity" link-type="inner">
                    <attribute name="ktr_answertext" alias="name" />
                    <filter type="and">
                        <condition attribute="ktr_managedlist" operator="eq" value="{MANAGED_LIST_ID}" />
                    </filter>
                </link-entity>
            </entity>
        </fetch>
`
};