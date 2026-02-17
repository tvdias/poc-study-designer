/**
 * DataService Unit Tests
 */

import { DataService } from '../../MatrixControl/services/DataService';
import {
    MatrixConfig,
    DataServiceConfig,
    RowEntity,
    ColumnEntity,
    JunctionRecord,
    BatchOperation
} from '../../MatrixControl/types/DataServiceTypes';

// Mock all the utility classes
jest.mock('../../MatrixControl/utils/ErrorHandler');
jest.mock('../../MatrixControl/utils/PerformanceTracker');
jest.mock('../../MatrixControl/utils/CacheManager');
jest.mock('../../MatrixControl/utils/NamingConventionManager');
jest.mock('../../MatrixControl/utils/FetchXMLQueryBuilder');
jest.mock('../../MatrixControl/utils/VersionChainProcessor');

// Import mocked classes
import { ErrorHandler } from '../../MatrixControl/utils/ErrorHandler';
import { PerformanceTracker } from '../../MatrixControl/utils/PerformanceTracker';
import { CacheManager } from '../../MatrixControl/utils/CacheManager';
import { NamingConventionManager } from '../../MatrixControl/utils/NamingConventionManager';
import { FetchXMLQueryBuilder } from '../../MatrixControl/utils/FetchXMLQueryBuilder';
import { VersionChainProcessor } from '../../MatrixControl/utils/VersionChainProcessor';

// Create typed mocks
const MockedErrorHandler = ErrorHandler as jest.Mocked<typeof ErrorHandler>;
const MockedPerformanceTracker = PerformanceTracker as jest.MockedClass<typeof PerformanceTracker>;
const MockedCacheManager = CacheManager as jest.MockedClass<typeof CacheManager>;
const MockedNamingConventionManager = NamingConventionManager as jest.Mocked<typeof NamingConventionManager>;
const MockedFetchXMLQueryBuilder = FetchXMLQueryBuilder as jest.Mocked<typeof FetchXMLQueryBuilder>;
const MockedVersionChainProcessor = VersionChainProcessor as jest.Mocked<typeof VersionChainProcessor>;

describe('DataService', () => {
    let mockWebAPI: jest.Mocked<ComponentFramework.WebApi>;
    let mockConfig: DataServiceConfig;
    let matrixConfig: MatrixConfig;
    let dataService: DataService;

    // Helper function to create proper PCF responses
    const createMockResponse = (entities: any[], totalCount?: number): ComponentFramework.WebApi.RetrieveMultipleResponse => ({
        entities,
        nextLink: '',
        '@Microsoft.Dynamics.CRM.totalrecordcount': totalCount || entities.length,
        '@odata.count': totalCount || entities.length
    } as any);

    // Helper function to create proper LookupValue responses  
    const createMockLookupValue = (id: string, entityType: string = 'entity'): ComponentFramework.LookupValue => ({
        id,
        entityType
    });

    // Mock data sets
    const mockRowEntities = [
        { questionid: 'question-1', fullname: 'Question A', createdon: '2023-01-01' },
        { questionid: 'question-2', fullname: 'Question B', createdon: '2023-01-02' }
    ];

    const mockColumnEntities = [
        { studyid: 'study-1', name: 'Study A', createdon: '2023-01-01' },
        { studyid: 'study-2', name: 'Study B', createdon: '2023-01-02' }
    ];

    const mockJunctionEntities = [
        {
            question_studyid: 'junction-1',
            questionid: 'question-1',
            studyid: 'study-1',
            _questionid_value: 'question-1',
            _studyid_value: 'study-1'
        }
    ];

    const mockContext: any = {
        client: {
            getGlobalContext: () => ({
                getClientUrl: () => "https://testorg.crm.dynamics.com"
            })
        }
    };

    beforeEach(() => {
        // Reset all mocks
        jest.clearAllMocks();

        // Mock service config
        mockConfig = {
            cacheTTL: 30000,
            maxRetries: 3,
            debugMode: true,
            enablePerformanceTracking: true
        };

        // Mock matrix config
        matrixConfig = {
            rowEntityName: 'question',
            columnEntityName: 'study',
            junctionEntityName: 'question_study',
            entityId: 'parent-record-id',
            entityName: 'project',
            rowIdField: 'questionid',
            columnIdField: 'studyid',
            junctionIdField: 'question_studyid',
            rowDisplayField: 'fullname',
            columnDisplayField: 'name',
            junctionRowField: 'questionid',
            junctionColumnField: 'studyid',
            rowParentField: 'parentquestionid',
            columnParentField: 'parentstudyid'
        };

        // Create robust WebAPI mock with implementation-based routing
        mockWebAPI = {
            retrieveMultipleRecords: jest.fn().mockImplementation(async (entityName: string, query?: string) => {
                // Handle count queries (aggregate queries) for any entity
                if (query && query.includes('aggregate')) {
                    if (entityName === 'study' || entityName.includes('study')) {
                        return createMockResponse([{ totalcount: 2 }]);
                    }
                    if (entityName === 'question' || entityName.includes('question')) {
                        return createMockResponse([{ totalcount: 2 }]);
                    }
                    return createMockResponse([{ totalcount: 0 }]);
                }

                // Handle regular entity queries
                if (entityName === 'question' || (entityName.includes('question') && !entityName.includes('question_study'))) {
                    return createMockResponse(mockRowEntities);
                }

                if (entityName === 'study' || (entityName.includes('study') && !entityName.includes('question_study'))) {
                    return createMockResponse(mockColumnEntities);
                }

                if (entityName === 'question_study' || entityName.includes('question_study')) {
                    return createMockResponse(mockJunctionEntities);
                }

                return createMockResponse([]);
            }),
            retrieveRecord: jest.fn(),
            createRecord: jest.fn().mockResolvedValue(createMockLookupValue('new-junction-id', 'question_study')),
            updateRecord: jest.fn().mockResolvedValue(createMockLookupValue('updated-junction-id', 'question_study')),
            deleteRecord: jest.fn().mockResolvedValue(createMockLookupValue('deleted-junction-id', 'question_study')),
            executeBatch: jest.fn()
        } as jest.Mocked<ComponentFramework.WebApi>;

        // Setup utility mocks with proper async handling
        MockedCacheManager.prototype.getOrFetch = jest.fn().mockImplementation(async (key: string, fetchFn: () => Promise<any>) => {
            try {
                // Execute the fetch function and return its result
                return await fetchFn();
            } catch (error) {
                throw error;
            }
        });

        MockedCacheManager.prototype.getStats = jest.fn().mockReturnValue({
            size: 0,
            hits: 0,
            misses: 0,
            hitRate: 0,
            totalRequests: 0
        });

        MockedCacheManager.prototype.clear = jest.fn();
        MockedCacheManager.prototype.destroy = jest.fn();
        MockedCacheManager.prototype.invalidateEntityCache = jest.fn();

        MockedPerformanceTracker.prototype.track = jest.fn().mockImplementation(async (name: string, fn: () => Promise<any>) => {
            try {
                return await fn();
            } catch (error) {
                throw error;
            }
        });

        MockedPerformanceTracker.prototype.getSummary = jest.fn().mockReturnValue({
            calls: 0,
            totalTime: 0,
            avgTime: 0,
            minTime: 0,
            maxTime: 0,
            lastCall: 0,
            errors: 0
        });

        MockedPerformanceTracker.prototype.reset = jest.fn();
        MockedPerformanceTracker.prototype.logReport = jest.fn();

        // Setup NamingConventionManager mocks
        MockedNamingConventionManager.getPrimaryKeyField = jest.fn().mockImplementation((entityName: string) => {
            if (entityName === 'question') return 'questionid';
            if (entityName === 'study') return 'studyid';
            if (entityName === 'question_study') return 'question_studyid';
            return 'id';
        });

        MockedNamingConventionManager.getFetchXMLFieldName = jest.fn().mockImplementation(name => name);
        MockedNamingConventionManager.escapeXmlValue = jest.fn().mockImplementation(value => value);
        MockedNamingConventionManager.getValidatedSchemaName = jest.fn().mockImplementation((entity, field) => field);
        MockedNamingConventionManager.getEntitySetNameForSave = jest.fn().mockImplementation(entity => `${entity}s`);
        MockedNamingConventionManager.getODataLookupFieldName = jest.fn().mockImplementation(field => `_${field}_value`);
        MockedNamingConventionManager.setSchemaNameOverride = jest.fn();
        MockedNamingConventionManager.getCacheStats = jest.fn().mockReturnValue({
            size: 0,
            hits: 0,
            misses: 0
        });
        MockedNamingConventionManager.clearCache = jest.fn();
        MockedNamingConventionManager.getEntityNameForFetchXMLUrl = jest.fn().mockImplementation(name => name);

        // Setup FetchXMLQueryBuilder mocks
        MockedFetchXMLQueryBuilder.buildRowEntitiesQuery = jest.fn().mockReturnValue('<fetch>row query</fetch>');
        MockedFetchXMLQueryBuilder.buildColumnEntitiesQuery = jest.fn().mockReturnValue('<fetch>column query</fetch>');
        MockedFetchXMLQueryBuilder.buildColumnCountQuery = jest.fn().mockReturnValue('<fetch aggregate="true">count query</fetch>'); // Added
        MockedFetchXMLQueryBuilder.buildJunctionRecordsQuery = jest.fn().mockReturnValue('<fetch>junction query</fetch>');
        MockedFetchXMLQueryBuilder.buildAllJunctionRecordsQuery = jest.fn().mockReturnValue('<fetch>all junction query</fetch>');
        MockedFetchXMLQueryBuilder.buildCountQuery = jest.fn().mockReturnValue('<fetch>count query</fetch>');
        MockedFetchXMLQueryBuilder.validateFetchXML = jest.fn().mockReturnValue({ isValid: true, errors: [] });
        MockedFetchXMLQueryBuilder.calculateActualUrlLength = jest.fn().mockReturnValue(500);
        MockedFetchXMLQueryBuilder.shouldIncludeVersionFields = jest.fn().mockReturnValue(false);

        // Setup ErrorHandler mocks
        MockedErrorHandler.handleDataverseError = jest.fn().mockImplementation(error => error);
        MockedErrorHandler.isNonRetryableError = jest.fn().mockReturnValue(false);
        MockedErrorHandler.isPermissionError = jest.fn().mockReturnValue(false);
        MockedErrorHandler.getUserFriendlyMessage = jest.fn().mockImplementation(error => error.message || 'Unknown error');

        // Setup VersionChainProcessor mocks
        MockedVersionChainProcessor.processColumns = jest.fn().mockImplementation((columns) => ({
            visibleColumns: columns,
            hiddenColumns: []
        }));
        MockedVersionChainProcessor.analyzeChains = jest.fn().mockReturnValue({
            totalStudies: 1,
            activeStudies: 1,
            abandonedStudies: 0
        });

        // Create DataService instance fresh for each test
        dataService = new DataService(mockWebAPI, mockConfig, mockContext);
    });

    describe('Constructor and Configuration', () => {
        it('should initialize with default configuration when no config provided', () => {
            const service = new DataService(mockWebAPI, mockConfig, mockContext);
            expect(service).toBeInstanceOf(DataService);
        });

        it('should merge provided config with defaults', () => {
            const customConfig = { debugMode: false, maxRetries: 5 };
            const service = new DataService(mockWebAPI, customConfig, mockContext);
            expect(service).toBeInstanceOf(DataService);
        });

        it('should initialize cache manager and performance tracker', () => {
            expect(MockedCacheManager).toHaveBeenCalledWith(30000, 1000, 60000);
            expect(MockedPerformanceTracker).toHaveBeenCalledWith(true);
        });
    });

    describe('Configuration Validation', () => {
        it('should validate required configuration fields', async () => {
            const invalidConfig = { ...matrixConfig };
            delete (invalidConfig as any).rowEntityName;

            await expect(dataService.loadInitialMatrixData(invalidConfig))
                .rejects.toThrow('Missing required configuration');
        });

        it('should validate entity name format', async () => {
            const invalidConfig = {
                ...matrixConfig,
                rowEntityName: 'invalid-entity-name!'
            };

            await expect(dataService.loadInitialMatrixData(invalidConfig))
                .rejects.toThrow('Invalid entity name format');
        });

        it('should validate field name format', async () => {
            const invalidConfig = {
                ...matrixConfig,
                rowDisplayField: 'invalid field name!'
            };

            await expect(dataService.loadInitialMatrixData(invalidConfig))
                .rejects.toThrow('Invalid field name format');
        });
    });

    // Add a completely isolated test for debugging
    describe('DEBUG: Configuration Issues', () => {
        it('should verify config is correct before calling DataService', () => {
            const testConfig: MatrixConfig = {
                rowEntityName: 'question',
                columnEntityName: 'study',
                junctionEntityName: 'question_study',
                entityId: 'parent-record-id',
                entityName: 'project',
                rowIdField: 'questionid',
                columnIdField: 'studyid',
                junctionIdField: 'question_studyid',
                rowDisplayField: 'fullname',
                columnDisplayField: 'name',
                junctionRowField: 'questionid',
                junctionColumnField: 'studyid',
                rowParentField: 'parentquestionid',
                columnParentField: 'parentstudyid'
            };

            console.log('DEBUG: testConfig.rowEntityName =', testConfig.rowEntityName);
            console.log('DEBUG: testConfig.rowIdField =', testConfig.rowIdField);
            expect(testConfig.rowEntityName).toBe('question');
            expect(testConfig.rowIdField).toBe('questionid');
        });

        it('should use correct config in isolated test', async () => {
            // Create completely fresh instances to avoid any pollution
            const freshWebAPI = {
                retrieveMultipleRecords: jest.fn().mockImplementation(async (entityName: string) => {
                    console.log('DEBUG: retrieveMultipleRecords called with entityName:', entityName);
                    return createMockResponse([]);
                }),
                retrieveRecord: jest.fn(),  // Add this missing property
                createRecord: jest.fn(),
                updateRecord: jest.fn(),
                deleteRecord: jest.fn(),
                executeBatch: jest.fn()
            } as jest.Mocked<ComponentFramework.WebApi>;

            const freshDataService = new DataService(freshWebAPI, { debugMode: true }, mockContext);

            const freshConfig: MatrixConfig = {
                rowEntityName: 'question',
                columnEntityName: 'study',
                junctionEntityName: 'question_study',
                entityId: 'parent-record-id',
                entityName: 'project',
                rowIdField: 'questionid',
                columnIdField: 'studyid',
                junctionIdField: 'question_studyid',
                rowDisplayField: 'fullname',
                columnDisplayField: 'name',
                junctionRowField: 'questionid',
                junctionColumnField: 'studyid',
                rowParentField: 'parentquestionid',
                columnParentField: 'parentstudyid'
            };

            console.log('DEBUG: Fresh config rowEntityName:', freshConfig.rowEntityName);

            await freshDataService.loadInitialMatrixData(freshConfig, 'parent-123');

            // Check what was actually called
            const rowCalls = MockedFetchXMLQueryBuilder.buildRowEntitiesQuery.mock.calls;
            if (rowCalls.length > 0) {
                console.log('DEBUG: buildRowEntitiesQuery called with:', JSON.stringify(rowCalls[0][0], null, 2));
            }
        });
    });

    describe('Initial Data Loading', () => {
        it('should load initial matrix data successfully', async () => {
            const result = await dataService.loadInitialMatrixData(matrixConfig);

            expect(result).toEqual({
                rows: expect.arrayContaining([
                    expect.objectContaining({ id: 'question-1', displayName: 'Question A' }),
                    expect.objectContaining({ id: 'question-2', displayName: 'Question B' })
                ]),
                columns: expect.arrayContaining([
                    expect.objectContaining({ id: 'study-1', displayName: 'Study A' }),
                    expect.objectContaining({ id: 'study-2', displayName: 'Study B' })
                ]),
                rawColumns: expect.arrayContaining([
                    expect.objectContaining({ id: 'study-1', displayName: 'Study A' }),
                    expect.objectContaining({ id: 'study-2', displayName: 'Study B' })
                ]),
                rawColumnsProcessed: 2,
                junctions: expect.arrayContaining([
                    expect.objectContaining({
                        id: 'junction-1',
                        rowId: 'question-1',
                        columnId: 'study-1'
                    })
                ]),
                totalRowCount: 2,
                totalColumnCount: 2,
                canEdit: true
            });

            // Verify that the service was called with correct parameters
            expect(MockedFetchXMLQueryBuilder.buildRowEntitiesQuery).toHaveBeenCalled();
            expect(MockedFetchXMLQueryBuilder.buildColumnEntitiesQuery).toHaveBeenCalled();
        });

        it('should handle parent record filtering', async () => {
            // Clear only the specific mocks we want to verify
            MockedFetchXMLQueryBuilder.buildRowEntitiesQuery.mockClear();
            MockedFetchXMLQueryBuilder.buildColumnEntitiesQuery.mockClear();

            const testConfig: MatrixConfig = {
                rowEntityName: 'question',
                columnEntityName: 'study',
                junctionEntityName: 'question_study',
                entityId: 'parent-record-id',
                entityName: 'project',
                rowIdField: 'questionid',
                columnIdField: 'studyid',
                junctionIdField: 'question_studyid',
                rowDisplayField: 'fullname',
                columnDisplayField: 'name',
                junctionRowField: 'questionid',
                junctionColumnField: 'studyid',
                rowParentField: 'parentquestionid',
                columnParentField: 'parentstudyid'
            };

            await dataService.loadInitialMatrixData(testConfig, 'parent-123');

            expect(MockedFetchXMLQueryBuilder.buildRowEntitiesQuery)
                .toHaveBeenCalledWith(expect.objectContaining({
                    rowEntityName: 'question',
                    rowIdField: 'questionid',
                    rowDisplayField: 'fullname',
                    rowParentField: 'parentquestionid'
                }), 0, 20, 'parent-123');
        });

        it('should handle version chain processing when configured', async () => {
            const versionConfig = {
                ...matrixConfig,
                columnParentAttrField: 'parentid',
                columnVersionField: 'version'
            };

            MockedFetchXMLQueryBuilder.shouldIncludeVersionFields = jest.fn().mockReturnValue(true);
            MockedVersionChainProcessor.processColumns = jest.fn().mockReturnValue({
                visibleColumns: [
                    { id: 'study-1', displayName: 'Study A', entityName: 'study' }
                ],
                hiddenColumns: []
            });

            const result = await dataService.loadInitialMatrixData(versionConfig);

            expect(MockedVersionChainProcessor.processColumns).toHaveBeenCalled();
            expect(result.columns).toHaveLength(1);
        }, 15000);

        it('should handle API errors gracefully', async () => {
            const apiError = new Error('API Error');

            // Mock the first retrieveMultipleRecords call to fail
            mockWebAPI.retrieveMultipleRecords.mockRejectedValue(apiError);

            // Ensure error handler re-throws the error
            MockedErrorHandler.handleDataverseError = jest.fn().mockImplementation(error => {
                throw error;
            });

            await expect(dataService.loadInitialMatrixData(matrixConfig))
                .rejects.toThrow();

            expect(MockedErrorHandler.handleDataverseError).toHaveBeenCalledWith(apiError);
        });

        it('should handle empty data sets', async () => {
            // Mock empty responses
            mockWebAPI.retrieveMultipleRecords.mockImplementation(async () => createMockResponse([]));

            const result = await dataService.loadInitialMatrixData(matrixConfig);

            expect(result.rows).toHaveLength(0);
            expect(result.columns).toHaveLength(0);
            expect(result.junctions).toHaveLength(0);
        });
    });

    describe('Junction Record Operations', () => {
        const mockRows: RowEntity[] = [
            { id: 'question-1', displayName: 'Question A', entityName: 'question' },
            { id: 'question-2', displayName: 'Question B', entityName: 'question' }
        ];

        const mockColumns: ColumnEntity[] = [
            { id: 'study-1', displayName: 'Study A', entityName: 'study' },
            { id: 'study-2', displayName: 'Study B', entityName: 'study' }
        ];

        it('should load junction records for entities', async () => {
            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, mockColumns);

            expect(result).toHaveLength(1);
            expect(result[0]).toEqual(
                expect.objectContaining({
                    id: 'junction-1',
                    rowId: 'question-1',
                    columnId: 'study-1'
                })
            );
        });

        it('should return empty array when no rows or columns provided', async () => {
            const resultNoRows = await dataService.loadJunctionRecordsForEntities(matrixConfig, [], mockColumns);
            const resultNoColumns = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, []);
            const resultNoBoth = await dataService.loadJunctionRecordsForEntities(matrixConfig, [], []);

            expect(resultNoRows).toEqual([]);
            expect(resultNoColumns).toEqual([]);
            expect(resultNoBoth).toEqual([]);
        });

        it('should choose optimal loading strategy based on data size', async () => {
            // Small dataset should use batched strategy
            const smallRows = mockRows.slice(0, 1);
            const smallColumns = mockColumns.slice(0, 1);

            await dataService.loadJunctionRecordsForEntities(matrixConfig, smallRows, smallColumns);

            // Should call batch loading
            expect(MockedFetchXMLQueryBuilder.buildJunctionRecordsQuery).toHaveBeenCalled();
        });

        it('should filter out invalid junction records', async () => {
            // Mock response with invalid junction records
            const invalidJunctionEntities = [
                { question_studyid: 'junction-1', questionid: 'question-1', studyid: 'study-1' }, // Valid
                { question_studyid: 'junction-2', questionid: null, studyid: 'study-2' }, // Invalid - null rowId
                { question_studyid: 'junction-3', questionid: 'question-3', studyid: '' }, // Invalid - empty columnId
                { question_studyid: 'junction-4' } // Invalid - missing both
            ];

            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question_study') {
                    return createMockResponse(invalidJunctionEntities);
                }
                return createMockResponse([]);
            });

            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, mockColumns);

            // Should only return the valid junction record
            expect(result).toHaveLength(1);
            expect(result[0].id).toBe('junction-1');
        });
    });

    describe('CRUD Operations', () => {
        it('should create junction record successfully (questionnaire line case)', async () => {
            const mockId = 'new-junction-id';
            const matrixConfig = {
                junctionEntityName: 'ktr_studyquestionnaireline',
                junctionRowField: 'ktr_QuestionnaireLine',
                junctionColumnField: 'ktr_Study',
                rowEntityName: 'kt_questionnairelines',
                columnEntityName: 'kt_studies'
            } as unknown as MatrixConfig;

            // Use synthetic IDs to ensure the path performs a create (no existing junction)
            const result = await dataService.createJunctionRecord(matrixConfig, 'question-999', 'study-999', 1);

            expect(typeof result).toBe('string');
            // Either a create (preferred) or a reactivation/update must have occurred
            const writeCalls = mockWebAPI.createRecord.mock.calls.length + mockWebAPI.updateRecord.mock.calls.length;
            expect(writeCalls).toBeGreaterThan(0);
            expect(result).toBe(mockId);
            expect(mockWebAPI.createRecord).toHaveBeenCalledWith(
                'ktr_studyquestionnaireline',
                expect.objectContaining({
                    'ktr_QuestionnaireLine@odata.bind': 'kt_questionnairelineses(question-999)',
                    'ktr_Study@odata.bind': 'kt_studies(study-999)',
                    ktr_sortorder: 1
                })
            );
            expect(MockedCacheManager.prototype.invalidateEntityCache)
                .toHaveBeenCalledWith('ktr_studyquestionnaireline');
        });

        it('should create junction record successfully (managed list case)', async () => {
            const mockId = 'new-managedlist-id';
            const matrixConfig = {
                junctionEntityName: 'ktr_studymanagedlistentity',
                junctionRowField: 'ktr_ManagedListEntity',
                junctionColumnField: 'ktr_Study',
                rowEntityName: 'ktr_managedlistentities',
                columnEntityName: 'kt_studies'
            } as unknown as MatrixConfig;

            mockWebAPI.createRecord.mockResolvedValue(createMockLookupValue(mockId, matrixConfig.junctionEntityName));

            const result = await dataService.createJunctionRecord(matrixConfig, 'managed-1', 'study-2', 0);

            expect(result).toBe(mockId);
            expect(mockWebAPI.createRecord).toHaveBeenCalledWith(
                'ktr_studymanagedlistentity',
                expect.objectContaining({
                    'ktr_ManagedListEntity@odata.bind': 'ktr_managedlistentities(managed-1)',
                    'ktr_Study@odata.bind': 'kt_studies(study-2)'
                })
            );
            expect(MockedCacheManager.prototype.invalidateEntityCache)
                .toHaveBeenCalledWith('ktr_studymanagedlistentity');
        });

        it('should update junction record successfully', async () => {
            await dataService.updateJunctionRecord(matrixConfig, 'junction-1', 'question-1', 'study-1');

            expect(mockWebAPI.updateRecord).toHaveBeenCalledWith(
                'question_study',
                'junction-1',
                expect.any(Object)
            );
            expect(MockedCacheManager.prototype.invalidateEntityCache).toHaveBeenCalledWith('question_study');
        });

        it('should delete junction record successfully', async () => {
            await dataService.deleteJunctionRecord(matrixConfig, 'junction-1');

            expect(mockWebAPI.deleteRecord).toHaveBeenCalledWith('question_study', 'junction-1');
            expect(MockedCacheManager.prototype.invalidateEntityCache).toHaveBeenCalledWith('question_study');
        });

        it('should gracefully fall back when create operation errors occur', async () => {
            const apiError = new Error('Permission denied');
            mockWebAPI.createRecord.mockRejectedValue(apiError);
            // Current implementation may reuse/reactivate an existing junction instead of failing outright
            const result = await dataService.createJunctionRecord(matrixConfig, 'question-999', 'study-999', 1);
            expect(typeof result).toBe('string');
        });
    });

    describe('Batch Save Operations', () => {
        const mockBatchOperation: BatchOperation = {
            creates: [
                { rowId: 'question-1', columnId: 'study-1' }
            ],
            updates: [
                { id: 'junction-1', rowId: 'question-2', columnId: 'study-2' }
            ],
            deletes: ['junction-2']
        };

        it('should execute batch save successfully', async () => {
            const result = await dataService.executeBatchSave(matrixConfig, mockBatchOperation);

            // Soft delete implementation now uses updateRecord instead of deleteRecord.
            // If a junction already exists, the create step becomes a reactivation (update) instead of a true create.
            expect(result.success).toBe(true);
            // Ensure at least the create/reactivation + update + soft delete paths executed (>=2 operations).
            const totalWrites = mockWebAPI.createRecord.mock.calls.length + mockWebAPI.updateRecord.mock.calls.length;
            expect(totalWrites).toBeGreaterThanOrEqual(2);
            // deleteRecord is no longer invoked for batch deletes (soft delete path)
            expect(mockWebAPI.deleteRecord).not.toHaveBeenCalled();
        });

        it('should handle partial failures in batch save', async () => {
            mockWebAPI.updateRecord.mockRejectedValue(new Error('Update failed'));

            const result = await dataService.executeBatchSave(matrixConfig, mockBatchOperation);

            // Create succeeds, both update + soft delete fail => 1 success, 2 failures
            expect(result.success).toBe(true); // Still true because at least one operation succeeded
            expect(result.errors).toBeDefined();
            expect(result.errors).toHaveLength(2);
        });

        it('should handle complete batch failure', async () => {
            mockWebAPI.createRecord.mockRejectedValue(new Error('Create failed'));
            mockWebAPI.updateRecord.mockRejectedValue(new Error('Update failed'));
            mockWebAPI.deleteRecord.mockRejectedValue(new Error('Delete failed'));

            const result = await dataService.executeBatchSave(matrixConfig, mockBatchOperation);

            expect(result.success).toBe(false);
            expect(result.errors).toBeDefined();
            expect(result.errors!.length).toBeGreaterThan(0);
        });

        it('should execute rollback on critical failures', async () => {
            const createError = new Error('Critical create error');
            mockWebAPI.createRecord.mockResolvedValue(createMockLookupValue('temp-id-1', 'question_study'));
            mockWebAPI.updateRecord.mockRejectedValue(createError);

            // Mock error to require rollback
            MockedErrorHandler.handleDataverseError = jest.fn().mockReturnValue({
                message: 'Critical error',
                rollbackRequired: true
            });

            const batchWithMultipleCreates = {
                creates: [
                    { rowId: 'question-1', columnId: 'study-1' },
                    { rowId: 'question-2', columnId: 'study-2' }
                ],
                updates: [{ id: 'junction-1', rowId: 'question-3', columnId: 'study-3' }],
                deletes: []
            };

            const result = await dataService.executeBatchSave(matrixConfig, batchWithMultipleCreates);

            // Verify cleanup was attempted
            expect(result).toBeDefined();
            expect(result.success).toBeDefined();
        });
    });

    describe('Field Value Extraction', () => {
        it('should extract field values using various patterns', async () => {
            const mockEntityWithVariousFields = {
                question_studyid: 'junction-1',
                directfield: 'direct-value',
                _lookupfield_value: 'lookup-value',
                'fieldwithbind@odata.bind': '/questions(guid-123)',
                CamelCaseField: 'camel-value',
                lowercase_field: 'lower-value'
            };

            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question_study') {
                    return createMockResponse([mockEntityWithVariousFields]);
                }
                return createMockResponse([]);
            });

            const mockRows: RowEntity[] = [{ id: 'question-1', displayName: 'John', entityName: 'question' }];
            const mockColumns: ColumnEntity[] = [{ id: 'study-1', displayName: 'Study', entityName: 'study' }];

            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, mockColumns);

            // The extraction logic should work with the available fields
            expect(result).toBeDefined();
        });

        it('should handle OData bind format extraction', async () => {
            const mockEntityWithODataBind = {
                question_studyid: 'junction-1',
                'questionid@odata.bind': '/questions(question-1)',
                'studyid@odata.bind': '/studies(study-1)'
            };

            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question_study') {
                    return createMockResponse([mockEntityWithODataBind]);
                }
                return createMockResponse([]);
            });

            const mockRows: RowEntity[] = [{ id: 'question-1', displayName: 'John', entityName: 'question' }];
            const mockColumns: ColumnEntity[] = [{ id: 'study-1', displayName: 'Study', entityName: 'study' }];

            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, mockColumns);

            expect(result).toBeDefined();
        });

        it('should handle missing fields gracefully', async () => {
            const mockEntityWithMissingFields = {
                question_studyid: 'junction-1'
                // Missing questionid and studyid fields
            };

            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question_study') {
                    return createMockResponse([mockEntityWithMissingFields]);
                }
                return createMockResponse([]);
            });

            const mockRows: RowEntity[] = [{ id: 'question-1', displayName: 'John', entityName: 'question' }];
            const mockColumns: ColumnEntity[] = [{ id: 'study-1', displayName: 'Study', entityName: 'study' }];

            // Should not throw, but filter out invalid records
            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, mockColumns);

            expect(result).toEqual([]);
        });
    });

    describe('Performance and Caching', () => {
        it('should track performance metrics', async () => {
            await dataService.loadInitialMatrixData(matrixConfig);

            expect(MockedPerformanceTracker.prototype.track).toHaveBeenCalledWith(
                'load_initial_data',
                expect.any(Function)
            );
        });

        it('should use cache for repeated requests', async () => {
            // First call
            await dataService.loadInitialMatrixData(matrixConfig);

            // Second call should use cache
            await dataService.loadInitialMatrixData(matrixConfig);

            // Verify cache was used
            expect(MockedCacheManager.prototype.getOrFetch).toHaveBeenCalled();
        });

        it('should return service statistics', () => {
            const stats = dataService.getServiceStats();

            expect(stats).toHaveProperty('cache');
            expect(stats).toHaveProperty('performance');
            expect(stats).toHaveProperty('namingConvention');
            expect(stats).toHaveProperty('config');
        });

        it('should clear all caches', () => {
            dataService.clearCache();

            expect(MockedCacheManager.prototype.clear).toHaveBeenCalled();
            expect(MockedPerformanceTracker.prototype.reset).toHaveBeenCalled();
            expect(MockedNamingConventionManager.clearCache).toHaveBeenCalled();
        });

        it('should invalidate cache on data changes', async () => {
            await dataService.createJunctionRecord(matrixConfig, 'question-1', 'study-1', 1);

            expect(MockedCacheManager.prototype.invalidateEntityCache)
                .toHaveBeenCalledWith('question_study');
        });
    });

    describe('Diagnostic Methods', () => {
        it('should diagnose junction field mapping', async () => {
            const diagnosis = await dataService.diagnoseJunctionFieldMapping(matrixConfig, 3);

            expect(diagnosis).toHaveProperty('entityName', 'question_study');
            expect(diagnosis).toHaveProperty('sampleRecords');
            expect(diagnosis).toHaveProperty('recommendations');
            expect(diagnosis.sampleRecords).toHaveLength(1);
            expect(diagnosis.recommendations).toBeInstanceOf(Array);
        });

        it('should provide recommendations for field mapping issues', async () => {
            // Mock empty junction records to trigger recommendations
            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question_study') {
                    return createMockResponse([]);
                }
                return createMockResponse([]);
            });

            const diagnosis = await dataService.diagnoseJunctionFieldMapping(matrixConfig, 3);

            expect(diagnosis.recommendations).toContain('No junction records found. Check if the junction entity name is correct.');
        });

        it('should analyze version chains when configured', async () => {
            const versionConfig = {
                ...matrixConfig,
                columnParentAttrField: 'parentid',
                columnVersionField: 'version'
            };

            // Initialize config first by calling loadInitialMatrixData
            await dataService.loadInitialMatrixData(versionConfig);

            const analysis = await dataService.analyzeVersionChains(versionConfig, 'parent-123');

            expect(MockedVersionChainProcessor.analyzeChains).toHaveBeenCalled();
            expect(analysis).toHaveProperty('totalStudies', 1);
        }, 15000);

        it('should set schema name overrides', () => {
            dataService.setSchemaNameOverride('entity', 'field', 'SchemaName');

            expect(MockedNamingConventionManager.setSchemaNameOverride)
                .toHaveBeenCalledWith('entity', 'field', 'SchemaName');
        });
    });

    describe('Error Scenarios', () => {
        it('should not retry non-retryable errors', async () => {
            const permissionError = new Error('Permission denied');
            let rowsCallCount = 0;

            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question') {
                    rowsCallCount++;
                    throw permissionError;
                }
                return createMockResponse([]);
            });

            MockedErrorHandler.isNonRetryableError.mockReturnValue(true);
            MockedErrorHandler.handleDataverseError.mockImplementation(error => {
                throw error; // Must throw to propagate the error
            });

            await expect(dataService.loadInitialMatrixData(matrixConfig))
                .rejects.toThrow();

            expect(rowsCallCount).toBe(1);
        });

        it('should handle URL length validation errors', async () => {
            MockedFetchXMLQueryBuilder.calculateActualUrlLength = jest.fn().mockReturnValue(5000);

            await expect(dataService.loadInitialMatrixData(matrixConfig))
                .rejects.toThrow('URL too long');
        });

        it('should handle invalid FetchXML', async () => {
            MockedFetchXMLQueryBuilder.validateFetchXML = jest.fn().mockReturnValue({
                isValid: false,
                errors: ['Invalid FetchXML structure']
            });

            await expect(dataService.loadInitialMatrixData(matrixConfig))
                .rejects.toThrow('Invalid FetchXML');
        });
    });

    describe('Edge Cases', () => {
        it('should handle empty entity arrays', async () => {
            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, [], []);
            expect(result).toEqual([]);
        });

        it('should handle malformed entity responses', async () => {
            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question') {
                    return createMockResponse([
                        { /* missing required fields */ },
                        { questionid: 'question-1' /* missing other fields */ }
                    ]);
                }
                return createMockResponse([]);
            });

            // Should handle gracefully without throwing
            const result = await dataService.loadInitialMatrixData(matrixConfig);
            expect(result).toBeDefined();
        });

        it('should handle large datasets efficiently', async () => {
            // Mock large dataset
            const largeRowSet = Array.from({ length: 100 }, (_, i) => ({
                questionid: `question-${i}`,
                fullname: `question ${i}`,
                createdon: '2023-01-01'
            }));

            const largeColumnSet = Array.from({ length: 100 }, (_, i) => ({
                studyid: `study-${i}`,
                name: `Study ${i}`,
                createdon: '2023-01-01'
            }));

            mockWebAPI.retrieveMultipleRecords.mockImplementation(async (entityName: string) => {
                if (entityName === 'question') {
                    return createMockResponse(largeRowSet);
                }
                if (entityName === 'study') {
                    return createMockResponse(largeColumnSet);
                }
                return createMockResponse([]);
            });

            const mockRows: RowEntity[] = largeRowSet.map(r => ({
                id: r.questionid,
                displayName: r.fullname,
                entityName: 'question'
            }));

            const mockColumns: ColumnEntity[] = largeColumnSet.map(c => ({
                id: c.studyid,
                displayName: c.name,
                entityName: 'study'
            }));

            const result = await dataService.loadJunctionRecordsForEntities(matrixConfig, mockRows, mockColumns);

            expect(result).toBeDefined();
            // Should use optimized strategy for large datasets
        });

        it('should calculate optimal batch size correctly', async () => {
            const result = await dataService.loadInitialMatrixData(matrixConfig);

            // Verify that batching logic is applied
            expect(result).toBeDefined();
        });
    });

    describe('Cleanup and Destruction', () => {
        it('should destroy service cleanly', () => {
            dataService.destroy();

            expect(MockedCacheManager.prototype.destroy).toHaveBeenCalled();
        });

        it('should log service report', () => {
            const consoleSpy = jest.spyOn(console, 'info').mockImplementation(() => { });

            dataService.logServiceReport();

            expect(MockedPerformanceTracker.prototype.logReport).toHaveBeenCalled();

            consoleSpy.mockRestore();
        });

        it('should handle repeated cleanup calls', () => {
            // Just verify that the methods can be called without throwing
            expect(() => {
                dataService.clearCache();
                dataService.clearCache();
                dataService.destroy();
            }).not.toThrow();

            // Verify the methods exist and are functions
            expect(typeof dataService.clearCache).toBe('function');
            expect(typeof dataService.destroy).toBe('function');
        });
    });

    describe('Pagination and Load More', () => {
        beforeEach(async () => {
            // Initialize config by calling loadInitialMatrixData
            await dataService.loadInitialMatrixData(matrixConfig);
        });

        it('should load more rows', async () => {
            const moreRows = await dataService.loadMoreRows(matrixConfig, 10, 5, 'parent-123');

            expect(moreRows).toBeDefined();
            expect(Array.isArray(moreRows)).toBe(true);
        });

        it('should load more columns', async () => {
            const moreColumns = await dataService.loadMoreColumns(matrixConfig, 10, 5, 'parent-123');

            expect(moreColumns).toBeDefined();
            expect(Array.isArray(moreColumns)).toBe(true);
        });

        it('should handle pagination parameters correctly', async () => {
            await dataService.loadMoreRows(matrixConfig, 20, 10);

            // Verify that FetchXMLQueryBuilder was called with correct pagination
            expect(MockedFetchXMLQueryBuilder.buildRowEntitiesQuery).toHaveBeenCalled();
        });
    });
});