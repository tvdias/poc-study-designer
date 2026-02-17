export class UrlHelper {

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
}