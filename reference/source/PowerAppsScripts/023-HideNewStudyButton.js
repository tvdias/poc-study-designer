function showNewStudyButtonOnProject(primaryControl) {
    if (!primaryControl) return false;

    var entityName = primaryControl.data.entity.getEntityName();

    return entityName !== "kt_study";  // Return false if entity is 'study'
}
