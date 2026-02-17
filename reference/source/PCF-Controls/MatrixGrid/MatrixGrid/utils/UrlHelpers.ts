import { IInputs } from "../generated/ManifestTypes";
import { EntityNamesConstants } from "./constants/EntityNamesConstants";

export class UrlHelpers {

    static navigateToEntityForm(entityId: string, context: ComponentFramework.Context<IInputs>, entityName: string): void {
        try {

            if(!entityName) {
                return;
            }

            context.navigation.openForm({
                entityName: entityName || '',
                entityId: entityId
            });
            console.log(`Navigating to ${entityName} record: ${entityId}`);
        } catch (error) {
            console.error('Error navigating to row record:', error);
            const fallbackUrl = `/main.aspx?etn=${entityName || ''}&id=${entityId}&pagetype=entityrecord`;
            window.open(fallbackUrl, '_blank');
            console.log(`Fallback navigation to: ${fallbackUrl}`);
        }
    }

    static getBaseUrl(context: ComponentFramework.Context<any>): string {
        
        let clientUrl = "";

        if ((context as any).client?.getGlobalContext?.()) {
            const globalContext = (context as any).client.getGlobalContext();
            clientUrl = globalContext.getClientUrl();
        } else if (window && (window as any).Xrm?.Utility?.getGlobalContext) {
            const globalContext = (window as any).Xrm.Utility.getGlobalContext();
            clientUrl = globalContext.getClientUrl();
        }

        return `${clientUrl}/api/data/v9.2`;
    }

    static getMainEntityName(entityName: string): string {
        switch (entityName) {
            case EntityNamesConstants.ktr_StudyManagedlistEntity:
                return EntityNamesConstants.ktr_ManagedListEntity;
            case EntityNamesConstants.ktr_StudyQuestionnaireline:
                return EntityNamesConstants.ktr_Questionnaireline;
            default:
                return '';
        }
    }
}