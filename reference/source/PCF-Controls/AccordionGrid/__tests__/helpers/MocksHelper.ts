import { DataService } from "../../AccordionGrid/services/DataService";

export class MocksHelper
{

    static getMockContext(): ComponentFramework.Context<any> {
        const mockContext = {
            navigation: { openForm: jest.fn() },
            parameters: {
                gridDataSet: {
                    records: [],
                    getSelectedRecordIds: jest.fn(),
                    getTargetEntityType: jest.fn(),
                    refresh: jest.fn(),
                }
            },
            webAPI: {},
            client: {
                getGlobalContext: jest.fn(() => ({
                    getClientUrl: jest.fn(() => "https://test-org.crm.dynamics.com"),
                    getCurrentAppProperties: jest.fn(() =>
                        Promise.resolve({ appId: "test-app-id-123" })
                    ),
                })),
            },
            userSettings: {
                securityRoles: ["3a27fcc5-cc0a-f011-bae2-000d3a2274a5"],
            }
        } as any;

        return mockContext;
    }

    static getMockDataService(): DataService {

        const mockDataService = {
            inactivateRecord: jest.fn(),
            reactivateRecord: jest.fn(),
            saveOrder: jest.fn(),
            loadInitialMatrixData: jest.fn(),
            loadMoreRows: jest.fn(),
            loadMoreColumns: jest.fn(),
            loadAllRows: jest.fn(),
            loadAllColumns: jest.fn(),
            loadJunctionRecordsForEntities: jest.fn(),
            executeBatchSave: jest.fn(),
            diagnoseJunctionFieldMapping: jest.fn(),
            analyzeVersionChains: jest.fn(),
            setSchemaNameOverride: jest.fn(),
            getServiceStats: jest.fn(),
            logServiceReport: jest.fn(),
            clearCache: jest.fn(),
            destroy: jest.fn(),
        } as unknown as DataService;

        return mockDataService;
    }
}