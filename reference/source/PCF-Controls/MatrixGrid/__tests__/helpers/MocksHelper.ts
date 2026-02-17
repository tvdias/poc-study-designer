import { MatrixService } from "../../MatrixGrid/services/MatrixService";

export class MocksHelper
{

    static getMockContext(): ComponentFramework.Context<any> {
        const mockContext = {
            navigation: { openForm: jest.fn() },
            parameters: {
                rowsEntityName: { raw: "ktr_row" },
                columnsEntityName: { raw: "ktr_col" },
                junctionEntityName: { raw: "ktr_junction" },
                matrixDataSet: { refresh: jest.fn() }
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

    static getMockMatrixService(): MatrixService {

        const mockMatrixService = {
            saveJunctionItems: jest.fn(),
        } as unknown as MatrixService;

        return mockMatrixService;
    }
}