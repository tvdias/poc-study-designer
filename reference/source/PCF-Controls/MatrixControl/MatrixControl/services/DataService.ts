/**
   * Get the actual junction ID field name (configured or auto-generated)
   */
import {
  MatrixConfig,
  RowEntity,
  ColumnEntity,
  JunctionRecord,
  BatchOperation,
  SaveResponse,
  DataverseEntity,
  FetchXMLResponse,
  ExtendedRetrieveMultipleResponse,
  LoadEntitiesResult,
  BatchOperationResult,
  DataServiceConfig
} from '../types/DataServiceTypes';

import { ErrorHandler } from '../utils/ErrorHandler';
import { PerformanceTracker } from '../utils/PerformanceTracker';
import { CacheManager } from '../utils/CacheManager';
import { NamingConventionManager } from '../utils/NamingConventionManager';
import { FetchXMLQueryBuilder } from '../utils/FetchXMLQueryBuilder';
import { VersionChainProcessor } from '../utils/VersionChainProcessor';
import { UrlHelper } from "../utils/UrlHelper";

export class DataService {
  private webAPI: ComponentFramework.WebApi;
  private navigationPropertyCache = new Map<string, string>();
  private cacheManager: CacheManager;
  private performanceTracker: PerformanceTracker;
  private config: Required<DataServiceConfig>;
  private context: ComponentFramework.Context<any>;

  // Configuration defaults
  private static readonly DEFAULT_CONFIG: Required<DataServiceConfig> = {
    cacheTTL: 30000,
    maxRetries: 3,
    maxUrlLength: 4000,
    debugMode: true,
    enablePerformanceTracking: true,
    cacheCleanupInterval: 60000,
    maxCacheSize: 1000
  };

  constructor(webAPI: ComponentFramework.WebApi, config: DataServiceConfig = {}, context: ComponentFramework.Context<any>) {
    this.webAPI = webAPI;
    this.config = { ...DataService.DEFAULT_CONFIG, ...config };
    this.context = context;

    this.cacheManager = new CacheManager(
      this.config.cacheTTL,
      this.config.maxCacheSize,
      this.config.cacheCleanupInterval
    );

    this.performanceTracker = new PerformanceTracker(this.config.enablePerformanceTracking);
  }

  /**
   * Enhanced debug logging with conditional output
   */
  private log(level: 'info' | 'warn' | 'error', message: string, data?: any): void {
    if (!this.config.debugMode && level === 'info') {
      return;
    }

    const prefix = level === 'error' ? 'E' : level === 'warn' ? 'W' : 'P';
    if (data) {
      console[level](`${prefix} DataService: ${message}`, data);
    } else {
      console[level](`${prefix} DataService: ${message}`);
    }
  }

  /**
   * Enhanced configuration validation
   */
  private validateConfig(config: MatrixConfig): void {
    const required = [
      'rowEntityName', 'columnEntityName', 'junctionEntityName',
      'rowDisplayField', 'columnDisplayField',
      'junctionRowField', 'junctionColumnField'
    ];

    const missing = required.filter(field => !config[field as keyof MatrixConfig]);
    if (missing.length > 0) {
      throw new Error(`Missing required configuration: ${missing.join(', ')}`);
    }

    // Enhanced validation
    const entityNamePattern = /^[a-z_][a-z0-9_]*$/i;
    const entities = [config.rowEntityName, config.columnEntityName, config.junctionEntityName];

    for (const entity of entities) {
      if (!entityNamePattern.test(entity)) {
        throw new Error(`Invalid entity name format: ${entity}`);
      }
    }

    const fieldNames = [
      config.rowDisplayField, config.columnDisplayField,
      config.junctionRowField, config.junctionColumnField
    ];

    for (const field of fieldNames) {
      if (!entityNamePattern.test(field)) {
        throw new Error(`Invalid field name format: ${field}`);
      }
    }
  }

  /**
   * Calculate optimal batch size based on URL constraints and data size
   */
  private calculateOptimalBatchSize(rowCount: number, columnCount: number): number {
    const baseUrlLength = 200;
    const availableSpace = this.config.maxUrlLength - baseUrlLength;

    const avgGuidLength = 36;
    const xmlOverheadPerValue = '<value></value>'.length;
    const spacePerId = avgGuidLength + xmlOverheadPerValue;

    const maxIdsInSingleQuery = Math.floor(availableSpace / (spacePerId * 2));
    const maxBatchSize = Math.floor(Math.sqrt(maxIdsInSingleQuery));

    // Consider data density - smaller batches for sparse matrices
    const density = Math.min(rowCount, columnCount) / Math.max(rowCount, columnCount, 1);
    const densityFactor = Math.max(0.3, density); // Minimum 30% of calculated size

    const optimalSize = Math.max(1, Math.min(maxBatchSize * densityFactor, 12));

    this.log('info', `Calculated optimal batch size: ${Math.floor(optimalSize)}`, {
      rowCount,
      columnCount,
      density,
      maxBatchSize,
      finalSize: Math.floor(optimalSize)
    });

    return Math.floor(optimalSize);
  }

  /**
   * Enhanced FetchXML execution with comprehensive retry logic
   */
  private async executeFetchXMLQuery<T extends DataverseEntity = DataverseEntity>(
    entityLogicalName: string,
    fetchXml: string,
    context: string
  ): Promise<FetchXMLResponse<T>> {

    return this.performanceTracker.track(`fetchxml_${context.toLowerCase()}`, async () => {
      // Validate FetchXML before execution
      const validation = FetchXMLQueryBuilder.validateFetchXML(fetchXml);
      if (!validation.isValid) {
        throw new Error(`Invalid FetchXML: ${validation.errors.join(', ')}`);
      }

      const entityNameForUrl = NamingConventionManager.getEntityNameForFetchXMLUrl(entityLogicalName);
      const actualUrlLength = FetchXMLQueryBuilder.calculateActualUrlLength(entityNameForUrl, fetchXml);

      if (actualUrlLength > this.config.maxUrlLength) {
        throw new Error(`URL too long (${actualUrlLength} chars) - exceeds limit of ${this.config.maxUrlLength}. Reduce batch size.`);
      }

      const queryUrl = `?fetchXml=${encodeURIComponent(fetchXml)}`;

      // Execute with retry logic
      let lastError: any = null;
      for (let attempt = 1; attempt <= this.config.maxRetries; attempt++) {
        try {
          const response = await this.webAPI.retrieveMultipleRecords(entityNameForUrl, queryUrl) as ExtendedRetrieveMultipleResponse;

          //response debugging - total count check
          console.log(`Debug api - ${context} debug end`, {
            allkeys: Object.keys(response),
            microsofttotalrecord: response['@Microsoft.Dynamics.CRM.totalrecordcount'],
            odatatotalrecord: response['@odata.count'],
            fullresponse: response
          })

          const typedResponse: FetchXMLResponse<T> = {
            entities: response.entities as T[],
            '@Microsoft.Dynamics.CRM.totalrecordcount': response['@Microsoft.Dynamics.CRM.totalrecordcount'],
            '@odata.count': response['@odata.count'],
            '@odata.nextLink': response['@odata.nextLink']
          };

          this.log('info', `${context} successful`, {
            entityCount: typedResponse.entities.length,
            totalCount: typedResponse['@Microsoft.Dynamics.CRM.totalrecordcount'] || typedResponse['@odata.count'],
            urlLength: actualUrlLength,
            attempt
          });

          return typedResponse;

        } catch (error) {
          lastError = ErrorHandler.handleDataverseError(error);

          if (ErrorHandler.isNonRetryableError(lastError)) {
            this.log('error', `${context} non-retryable error`, lastError);
            throw lastError;
          }

          if (attempt < this.config.maxRetries) {
            const delay = Math.min(1000 * Math.pow(2, attempt - 1), 5000);
            this.log('warn', `${context} retry ${attempt}/${this.config.maxRetries} after ${delay}ms`, {
              error: lastError.message,
              attempt,
              delay
            });
            await new Promise(resolve => setTimeout(resolve, delay));
          }
        }
      }

      this.log('error', `${context} failed after ${this.config.maxRetries} attempts`, lastError);
      throw lastError;
    });
  }

  /**
   * Enhanced navigation property cache initialization with fallback
   */
  async initializeNavigationPropertyCache(config: MatrixConfig): Promise<void> {
    try {
      this.log('info', 'Initializing navigation property cache...');

      // Use logical names directly as fallback approach
      this.navigationPropertyCache.set(
        `${config.junctionEntityName}.${config.junctionRowField}`,
        config.junctionRowField
      );

      this.navigationPropertyCache.set(
        `${config.junctionEntityName}.${config.junctionColumnField}`,
        config.junctionColumnField
      );

      this.log('info', 'Navigation property cache initialized', {
        rowField: config.junctionRowField,
        columnField: config.junctionColumnField
      });

    } catch (error) {
      this.log('warn', 'Navigation property cache initialization failed, using fallbacks', error);

      // Ensure fallbacks are set even if initialization fails
      this.navigationPropertyCache.set(`${config.junctionEntityName}.${config.junctionRowField}`, config.junctionRowField);
      this.navigationPropertyCache.set(`${config.junctionEntityName}.${config.junctionColumnField}`, config.junctionColumnField);
    }
  }

  /**
   * Get schema name for save operations with enhanced validation
   */
  private getSchemaNameForSave(entityName: string, fieldName: string): string {
    const cacheKey = `${entityName}.${fieldName}`;
    let schemaName = this.navigationPropertyCache.get(cacheKey);

    if (!schemaName) {
      schemaName = NamingConventionManager.getValidatedSchemaName(entityName, fieldName);
      this.navigationPropertyCache.set(cacheKey, schemaName);
    }

    return schemaName;
  }

  /**
   * Manual override for schema names - improved version
   */
  public setSchemaNameOverride(entityName: string, fieldLogicalName: string, schemaName: string): void {
    const cacheKey = `${entityName}.${fieldLogicalName}`;
    this.navigationPropertyCache.set(cacheKey, schemaName);

    // Also update the naming convention manager
    NamingConventionManager.setSchemaNameOverride(entityName, fieldLogicalName, schemaName);

    this.log('info', `Schema name override applied`, {
      entity: entityName,
      field: fieldLogicalName,
      schemaName: schemaName
    });
  }

  private async getTotalColumnCount(
    config: MatrixConfig,
    parentRecordId?: string
  ): Promise<number> {
    try {
      const countFetchXml = FetchXMLQueryBuilder.buildColumnCountQuery(config, parentRecordId);

      const response = await this.executeFetchXMLQuery(config.columnEntityName, countFetchXml, 'COUNT COLUMNS');
      const rawTotalCount = response.entities[0]?.totalcount || 0;

      this.log('info', `Raw total column count: ${rawTotalCount}`);

      const hasVersionChainFeatures = !!(config.columnParentAttrField || config.columnVersionField);

      if (hasVersionChainFeatures) {
        const sampleSize = Math.min(50, rawTotalCount);
        if (sampleSize > 0) {
          this.log('info', 'Sampling columns using loadEntitiesRaw for accurate version-processed count', {
            sampleSize,
            parentRecordId,
            columnEntity: config.columnEntityName
          });

          // Use loadEntitiesRaw so the sample uses the same FetchXML + version processing as the main load
          const sampleResult = await this.loadEntitiesRaw<ColumnEntity>(
            config.columnEntityName,
            config.columnIdField,
            config.columnDisplayField,
            config.columnParentField,
            0,
            sampleSize,
            parentRecordId,
            'columns'
          );

          const processedSample = sampleResult.processedEntities.length;
          const processingRatio = processedSample / sampleSize;
          const estimatedTotal = Math.ceil(rawTotalCount * processingRatio);

          this.log('info', 'Estimated total with version chains', {
            rawTotal: rawTotalCount,
            sampleSize,
            processedSample,
            processingRatio,
            estimatedTotal
          });

          // If processedSample is zero (unexpected) fall back to rawTotalCount rather than returning 0
          if (processedSample === 0) {
            this.log('warn', 'Sample processed count is zero — falling back to raw total to avoid underestimating', {
              rawTotalCount,
              sampleSize
            });
            return rawTotalCount;
          }

          return estimatedTotal;
        }
      }

      return rawTotalCount;
    } catch (error) {
      this.log('warn', 'Failed to get total column count', error);
      return 0;
    }
  }

  /**
  * Enhanced initial data loading with better error handling and performance tracking
  */
  async loadInitialMatrixData(config: MatrixConfig, parentRecordId?: string): Promise<{
    rows: RowEntity[];
    columns: ColumnEntity[];
    rawColumns: ColumnEntity[]; // Return raw columns too
    junctions: JunctionRecord[];
    totalRowCount: number;
    totalColumnCount: number;
    canEdit: boolean;
    rawColumnsProcessed: number;
  }> {

    return this.performanceTracker.track('load_initial_data', async () => {
      try {
        this.validateConfig(config);

        // Store config for use in loadEntities
        this.setCurrentConfig(config);

        await this.initializeNavigationPropertyCache(config);

        const pageSize = config.pageSize || 20;

        const totalColumnCount = await this.getTotalColumnCount(config, parentRecordId);

        // Load data and check permissions in parallel
        const [rowsResult, columnsResult, canEdit] = await Promise.all([
          this.loadEntities<RowEntity>(
            config.rowEntityName,
            config.rowIdField,
            config.rowDisplayField,
            config.rowParentField,
            0,
            pageSize,
            parentRecordId,
            'rows'
          ),
          this.loadEntitiesRaw<ColumnEntity>( // Use special method that returns both raw and processed
            config.columnEntityName,
            config.columnIdField,
            config.columnDisplayField,
            config.columnParentField,
            0,
            pageSize,
            parentRecordId,
            'columns'
          ),
          this.checkEntityPrivileges(config.junctionEntityName, 'Create')
        ]);

        // Extract both raw and processed columns
        const rawColumns = columnsResult.rawEntities;
        const processedColumns = columnsResult.processedEntities;
        const rawColumnsProcessed = rawColumns.length;

        // Log enhanced loading info
        const hasVersionFields = !!(config.columnParentAttrField || config.columnVersionField);
        this.log('info', 'Initial data loaded', {
          rowCount: rowsResult.entities.length,
          rawColumnCount: rawColumns.length,
          processedColumnCount: processedColumns.length,
          rawColumnsProcessed: rawColumnsProcessed,
          totalRows: rowsResult.totalCount,
          totalColumns: totalColumnCount,
          canEdit,
          versionChainProcessing: hasVersionFields
        });

        // Load junction records using processed columns
        const junctions = await this.loadJunctionRecordsForEntities(config, rowsResult.entities, processedColumns);

        return {
          rows: rowsResult.entities,
          columns: processedColumns, // Return processed visible columns
          rawColumns: rawColumns, // Return raw columns for pagination tracking
          junctions,
          totalRowCount: rowsResult.totalCount,
          totalColumnCount,
          canEdit,
          rawColumnsProcessed
        };

      } catch (error) {
        // Error handling unchanged
        const handledError = ErrorHandler.handleDataverseError(error);
        this.log('error', 'Failed to load initial matrix data', handledError);
        throw new Error(`Failed to load matrix data: ${ErrorHandler.getUserFriendlyMessage(handledError)}`);
      }
    });
  }

  /**
   * Load entities and return both raw and processed versions for columns
   * This method is now public for direct use by MatrixContainer
   */
  public async loadEntitiesRaw<T extends ColumnEntity>(
    entityName: string,
    idField: string | undefined,
    displayField: string,
    parentField: string,
    skip: number,
    take: number,
    parentRecordId: string | undefined,
    entityType: 'columns'
  ): Promise<{
    rawEntities: T[];
    processedEntities: T[];
    totalCount: number;
    hasMore: boolean;
  }> {
    const cacheKey = `entities_raw:${entityName}:${skip}:${take}:${parentRecordId || 'no-parent'}`;

    return this.cacheManager.getOrFetch(cacheKey, async () => {
      const actualIdField = idField || NamingConventionManager.getPrimaryKeyField(entityName);
      const config = this.getCurrentConfig();
      const includeVersionFields = FetchXMLQueryBuilder.shouldIncludeVersionFields(config);

      // Build and execute query
      const fetchXml = FetchXMLQueryBuilder.buildColumnEntitiesQuery(
        {
          ...config,
          columnEntityName: entityName,
          columnIdField: actualIdField,
          columnDisplayField: displayField,
          columnParentField: parentField,
        },
        skip,
        take,
        parentRecordId,
        [],
        includeVersionFields
      );

      const response = await this.executeFetchXMLQuery<DataverseEntity>(
        entityName,
        fetchXml,
        `LOAD ${entityType.toUpperCase()} RAW`
      );

      // Map raw entities with enhanced field extraction
      const rawEntities = response.entities.map((entity: DataverseEntity) => {
        let displayName = entity[displayField] || 'Unnamed';

        const versionValue = (includeVersionFields && config.columnVersionField)
          ? entity[config.columnVersionField] ||
          entity[NamingConventionManager.getFetchXMLFieldName(config.columnVersionField)]
          : undefined;

        if (versionValue) {
          const versionPrefix = versionValue.toString().startsWith('v') ? versionValue : `V${versionValue}`;
          displayName = `(${versionPrefix}) - ${displayName}`;
        }

        return {
          id: entity[actualIdField],
          displayName: displayName,
          createdDate: entity.createdon ? new Date(entity.createdon) : new Date(),
          ...(includeVersionFields ? {
            statuscode: entity.statuscode,
            parentAttrId: config.columnParentAttrField
              ? this.extractFieldValue(entity, config.columnParentAttrField)
              : undefined,
            versionValue: versionValue
          } : {}),
          ...entity
        };
      }) as T[];

      // Process through version chains if configured
      let processedEntities = rawEntities;
      if (includeVersionFields) {
        const columnEntities = rawEntities as unknown as ColumnEntity[];

        this.log('info', 'Processing version chains for raw columns', {
          rawColumnCount: columnEntities.length,
          hasParentAttrField: !!config.columnParentAttrField,
          hasVersionField: !!config.columnVersionField
        });

        const { visibleColumns } = VersionChainProcessor.processColumns(columnEntities, config);

        this.log('info', 'Version chain processing complete', {
          originalCount: columnEntities.length,
          visibleCount: visibleColumns.length,
          filtered: columnEntities.length - visibleColumns.length
        });

        processedEntities = visibleColumns as unknown as T[];
      }

      const totalCount =
        response['@Microsoft.Dynamics.CRM.totalrecordcount'] ||
        response['@odata.count'] ||
        rawEntities.length;

      const hasMore = skip + take < totalCount;

      return {
        rawEntities,
        processedEntities,
        totalCount,
        hasMore
      };
    });
  }

  /**
   * NEW: Debug method for version chain analysis
   */
  public async analyzeVersionChains(config: MatrixConfig, parentRecordId?: string): Promise<any> {
    try {
      // Load raw columns without version processing
      const tempConfig = { ...config };
      delete (tempConfig as any).columnParentAttrField;
      delete (tempConfig as any).columnVersionField;

      const rawColumnsResult = await this.loadEntities<ColumnEntity>(
        config.columnEntityName,
        config.columnIdField,
        config.columnDisplayField,
        config.columnParentField,
        0,
        1000, // Get more records for analysis
        parentRecordId,
        'columns'
      );

      return VersionChainProcessor.analyzeChains(rawColumnsResult.entities, config);

    } catch (error) {
      const handledError = ErrorHandler.handleDataverseError(error);
      this.log('error', 'Version chain analysis failed', handledError);
      throw handledError;
    }
  }

  /**
   * Generic entity loading with enhanced caching and error handling
   */
  private async loadEntities<T extends RowEntity | ColumnEntity>(
    entityName: string,
    idField: string | undefined,
    displayField: string,
    parentField: string | undefined,
    skip: number,
    take: number,
    parentRecordId: string | undefined,
    entityType: 'rows' | 'columns'
  ): Promise<LoadEntitiesResult<T>> {


    const cacheKey = `entities:${entityName}:${skip}:${take}:${parentRecordId || 'no-parent'}`;

    return this.cacheManager.getOrFetch(cacheKey, async () => {
      const actualIdField = idField || NamingConventionManager.getPrimaryKeyField(entityName);

      // NEW: Auto-detect if we should include version fields (only for columns)
      const config = this.getCurrentConfig(); // We need to pass config to this method
      const includeVersionFields = entityType === 'columns' &&
        FetchXMLQueryBuilder.shouldIncludeVersionFields(config);

      let fetchXml: string;

      if (entityType === 'rows') {
        fetchXml = FetchXMLQueryBuilder.buildRowEntitiesQuery({
          rowEntityName: entityName,
          rowIdField: actualIdField,
          rowDisplayField: displayField,
          rowParentField: parentField
        } as MatrixConfig, skip, take, parentRecordId);
      } else {
        fetchXml = FetchXMLQueryBuilder.buildColumnEntitiesQuery({
          columnEntityName: entityName,
          columnIdField: actualIdField,
          columnDisplayField: displayField,
          columnParentField: parentField,
          columnParentAttrField: config.columnParentAttrField,
          columnVersionField: config.columnVersionField
        } as MatrixConfig, skip, take, parentRecordId, [], includeVersionFields);
      }

      const response = await this.executeFetchXMLQuery<DataverseEntity>(entityName, fetchXml, `LOAD ${entityType.toUpperCase()}`);

      // Map entities with enhanced field extraction
      let rawEntities = response.entities.map((entity: DataverseEntity) => {
        // Get base display name
        let displayName = entity[displayField] || 'Unnamed';

        // Extract version value for columns when version fields are included
        const versionValue = (entityType === 'columns' && includeVersionFields && config.columnVersionField) ?
          entity[config.columnVersionField] || entity[NamingConventionManager.getFetchXMLFieldName(config.columnVersionField)] :
          undefined;

        // For columns with version values, prepend version to display name
        if (versionValue) {
          // Format as "V2 Study Name" - add V prefix if not already present
          const versionPrefix = versionValue.toString().startsWith('v') ? versionValue : `V${versionValue}`;
          displayName = `(${versionPrefix}) - ${displayName}`;
        }

        return {
          id: entity[actualIdField],
          displayName: displayName,  // Now includes version prefix when available
          createdDate: entity.createdon ? new Date(entity.createdon) : new Date(),
          sortOrder: entity.kt_questionsortorder || 0,

          // NEW: Include version-related fields when available (for columns)
          ...(entityType === 'columns' && includeVersionFields ? {
            statuscode: entity.statuscode,
            parentAttrId: config.columnParentAttrField ?
              this.extractFieldValue(entity, config.columnParentAttrField) :
              undefined,
            versionValue: versionValue  // Reuse the extracted version value
          } : {}),

          ...entity
        };
      }) as T[];

      // NEW: Process version chains for columns when version fields are configured
      if (entityType === 'columns' && includeVersionFields) {
        const columnEntities = rawEntities as unknown as ColumnEntity[];

        this.log('info', 'Processing version chains for columns', {
          rawColumnCount: columnEntities.length,
          hasParentAttrField: !!config.columnParentAttrField,
          hasVersionField: !!config.columnVersionField
        });

        const { visibleColumns } = VersionChainProcessor.processColumns(columnEntities, config);

        this.log('info', 'Version chain processing complete', {
          originalCount: columnEntities.length,
          visibleCount: visibleColumns.length,
          filtered: columnEntities.length - visibleColumns.length
        });

        rawEntities = visibleColumns as unknown as T[];
      }

      const totalCount = response['@Microsoft.Dynamics.CRM.totalrecordcount'] ||
        response['@odata.count'] ||
        rawEntities.length; // Use processed count for version chains

      const hasMore = skip + take < totalCount;

      console.log(`[LoadEntities] EntityType: ${entityType}, TotalCount: ${totalCount}, HasMore: ${hasMore}`);

      return {
        entities: rawEntities,
        totalCount,
        hasMore,
        nextPageInfo: hasMore ? { skip: skip + take, take } : undefined
      };
    });
  }

  /**
   * NEW: Helper method to get current MatrixConfig
   * This needs to be added to store the config for use in loadEntities
   */
  private currentConfig: MatrixConfig | null = null;

  /**
   * NEW: Method to set current config (called from loadInitialMatrixData)
   */
  private setCurrentConfig(config: MatrixConfig): void {
    this.currentConfig = config;
  }

  /**
   * NEW: Method to get current config safely
   */
  private getCurrentConfig(): MatrixConfig {
    if (!this.currentConfig) {
      throw new Error('DataService config not initialized');
    }
    return this.currentConfig;
  }


  /**
   * Enhanced junction record loading with improved strategy selection and field mapping
   */
  async loadJunctionRecordsForEntities(config: MatrixConfig, rows: RowEntity[], columns: ColumnEntity[]): Promise<JunctionRecord[]> {
    if (rows.length === 0 || columns.length === 0) {
      return [];
    }

    return this.performanceTracker.track('load_junction_records', async () => {
      try {
        const batchSize = this.calculateOptimalBatchSize(rows.length, columns.length);
        const totalPossibleCalls = Math.ceil(rows.length / batchSize) * Math.ceil(columns.length / batchSize);

        // Enhanced strategy selection based on data characteristics
        if (totalPossibleCalls > 10 || (rows.length * columns.length) > 150) {
          this.log('info', 'Using optimized junction loading strategy');
          return await this.loadJunctionRecordsOptimized(config, rows, columns);
        } else {
          this.log('info', 'Using batched junction loading strategy');
          return await this.loadJunctionRecordsBatched(config, rows, columns);
        }

      } catch (error) {
        const handledError = ErrorHandler.handleDataverseError(error);
        this.log('error', 'Failed to load junction records', handledError);
        throw new Error(`Failed to load junction records: ${ErrorHandler.getUserFriendlyMessage(handledError)}`);
      }
    });
  }

  /**
   * Optimized junction loading - loads all junctions then filters in memory
   */
  private async loadJunctionRecordsOptimized(config: MatrixConfig, rows: RowEntity[], columns: ColumnEntity[]): Promise<JunctionRecord[]> {
    const allJunctions = await this.loadAllJunctionRecords(config, columns);

    // Create lookup sets for O(1) filtering performance
    const rowIdSet = new Set(rows.map(r => r.id));
    const columnIdSet = new Set(columns.map(c => c.id));

    // Filter for junctions that match our current rows and columns
    // Note: allJunctions already contains only valid records (filtered in loadAllJunctionRecords)
    const filteredJunctions = allJunctions.filter(junction =>
      rowIdSet.has(junction.rowId) && columnIdSet.has(junction.columnId)
    );

    this.log('info', 'Optimized junction filtering completed', {
      totalJunctions: allJunctions.length,
      filteredJunctions: filteredJunctions.length,
      filterEfficiency: `${((filteredJunctions.length / Math.max(allJunctions.length, 1)) * 100).toFixed(1)}%`
    });

    return filteredJunctions;
  }

  /**
   * Enhanced batched junction loading with improved field mapping and error recovery
   */
  private async loadJunctionRecordsBatched(config: MatrixConfig, rows: RowEntity[], columns: ColumnEntity[]): Promise<JunctionRecord[]> {
    const batchSize = this.calculateOptimalBatchSize(rows.length, columns.length);
    const allJunctions: JunctionRecord[] = [];
    const errors: string[] = [];

    this.log('info', 'Starting batched junction loading', {
      rowCount: rows.length,
      columnCount: columns.length,
      batchSize,
      estimatedBatches: Math.ceil(rows.length / batchSize) * Math.ceil(columns.length / batchSize)
    });

    for (let i = 0; i < rows.length; i += batchSize) {
      const rowBatch = rows.slice(i, i + batchSize);

      for (let j = 0; j < columns.length; j += batchSize) {
        const columnBatch = columns.slice(j, j + batchSize);

        try {
          const batchJunctions = await this.loadJunctionBatch(config, rowBatch, columnBatch);
          allJunctions.push(...batchJunctions);
        } catch (batchError) {
          const handledError = ErrorHandler.handleDataverseError(batchError);
          const errorMsg = `Batch failed (rows ${i}-${i + rowBatch.length}, cols ${j}-${j + columnBatch.length}): ${handledError.message}`;
          errors.push(errorMsg);
          this.log('warn', errorMsg);

          // Continue with other batches even if this one fails
        }
      }
    }

    if (errors.length > 0 && allJunctions.length === 0) {
      throw new Error(`All junction batches failed: ${errors.join('; ')}`);
    }

    this.log('info', 'Batched junction loading completed', {
      totalJunctions: allJunctions.length,
      validJunctions: allJunctions.filter(j => j.rowId && j.columnId).length,
      batchErrors: errors.length
    });

    return allJunctions;
  }

  /**
   * Load a single junction batch with enhanced field mapping and validation
   */
  private async loadJunctionBatch(config: MatrixConfig, rows: RowEntity[], columns: ColumnEntity[]): Promise<JunctionRecord[]> {
    const fetchXml = FetchXMLQueryBuilder.buildJunctionRecordsQuery(
      config,
      rows.map(r => r.id),
      columns.map(c => c.id)
    );

    const response = await this.executeFetchXMLQuery<DataverseEntity>(
      config.junctionEntityName,
      fetchXml,
      'LOAD JUNCTION BATCH'
    );

    const junctionIdField = this.getJunctionIdField(config);

    // Map all records first
    const allRecords = response.entities.map((entity: DataverseEntity) => ({
      id: entity[junctionIdField],
      rowId: this.extractFieldValue(entity, config.junctionRowField),
      columnId: this.extractFieldValue(entity, config.junctionColumnField),
      ...entity
    }));

    // Filter out junction records that don't have valid rowId and columnId
    const validJunctions = allRecords.filter((record): record is JunctionRecord =>
      record.rowId !== undefined &&
      record.columnId !== undefined &&
      record.rowId !== null &&
      record.columnId !== null &&
      record.rowId.trim() !== '' &&
      record.columnId.trim() !== ''
    );

    const invalidCount = allRecords.length - validJunctions.length;
    if (invalidCount > 0) {
      this.log('warn', `Batch contained ${invalidCount} invalid junction records`, {
        batchTotal: allRecords.length,
        validInBatch: validJunctions.length,
        invalidInBatch: invalidCount
      });
    }

    return validJunctions;
  }

  /**
   * Enhanced field value extraction with multiple pattern matching and debugging
   */
  private extractFieldValue(entity: DataverseEntity, fieldName: string): string | undefined {
    const possibleFieldNames = [
      fieldName,                              // Direct field name
      `_${fieldName}_value`,                  // Lookup field with _value suffix
      `${fieldName}@odata.bind`,              // OData bind format
      fieldName.toLowerCase(),                // Lowercase version
      fieldName.toUpperCase(),                // Uppercase version
      NamingConventionManager.getODataLookupFieldName(fieldName) // Standardized lookup field
    ];

    for (let i = 0; i < possibleFieldNames.length; i++) {
      const possibleField = possibleFieldNames[i];
      const value = entity[possibleField];

      if (value !== undefined && value !== null) {
        // Extract GUID from OData bind format if needed
        if (typeof value === 'string' && value.includes('(') && value.includes(')')) {
          const match = value.match(/\(([^)]+)\)/);
          const extractedValue = match ? match[1] : value;

          if (this.config.debugMode) {
            this.log('info', `Field extraction successful`, {
              fieldName,
              pattern: ['direct', '_value', '@odata.bind', 'lowercase', 'uppercase', 'standardized'][i],
              originalValue: value,
              extractedValue
            });
          }

          return extractedValue;
        }

        if (this.config.debugMode) {
          this.log('info', `Field extraction successful`, {
            fieldName,
            pattern: ['direct', '_value', '@odata.bind', 'lowercase', 'uppercase', 'standardized'][i],
            value
          });
        }

        return String(value);
      }
    }

    // If we reach here, field extraction failed
    if (this.config.debugMode) {
      this.log('warn', `Field extraction failed for ${fieldName}`, {
        fieldName,
        availableFields: Object.keys(entity),
        attemptedPatterns: possibleFieldNames
      });
    }

    return undefined;
  }

  /**
   * Load all junction records with caching and validation
   */
  private async loadAllJunctionRecords(config: MatrixConfig, columns: ColumnEntity[]): Promise<JunctionRecord[]> {
    const cacheKey = `all_junctions:${config.junctionEntityName}`;

    return this.cacheManager.getOrFetch(cacheKey, async () => {
      const fetchXml = FetchXMLQueryBuilder.buildAllJunctionRecordsQuery(
        config,
        columns.map(c => c.id));
      const response = await this.executeFetchXMLQuery<DataverseEntity>(
        config.junctionEntityName,
        fetchXml,
        'LOAD ALL JUNCTIONS'
      );

      const junctionIdField = this.getJunctionIdField(config);

      // Map and filter out invalid junction records
      const allRecords = response.entities.map((entity: DataverseEntity) => ({
        id: entity[junctionIdField],
        rowId: this.extractFieldValue(entity, config.junctionRowField),
        columnId: this.extractFieldValue(entity, config.junctionColumnField),
        ...entity
      }));

      // Filter out junction records that don't have valid rowId and columnId
      const validJunctions = allRecords.filter((record): record is JunctionRecord =>
        record.rowId !== undefined &&
        record.columnId !== undefined &&
        record.rowId !== null &&
        record.columnId !== null &&
        record.rowId.trim() !== '' &&
        record.columnId.trim() !== ''
      );

      const invalidCount = allRecords.length - validJunctions.length;
      if (invalidCount > 0) {
        this.log('warn', `Filtered out ${invalidCount} invalid junction records`, {
          total: allRecords.length,
          valid: validJunctions.length,
          invalid: invalidCount
        });
      }

      return validJunctions;
    });
  }
  private getJunctionIdField(config: MatrixConfig): string {
    // Use configured junction ID field if provided, otherwise auto-generate
    return config.junctionIdField || NamingConventionManager.getPrimaryKeyField(config.junctionEntityName);
  }
  /**
   * Enhanced batch save with rollback capability and detailed error reporting
   */
  async executeBatchSave(config: MatrixConfig, operation: BatchOperation): Promise<SaveResponse> {
    return this.performanceTracker.track('batch_save', async () => {
      const results: BatchOperationResult[] = [];
      const createdIds: string[] = [];

      try {
        // Execute creates first and track for potential rollback
        for (const record of operation.creates) {
          try {
            const id = await this.createJunctionRecord(config, record.rowId, record.columnId, record.sortOrder);
            createdIds.push(id);
            results.push({ operation: 'create', id, success: true });
          } catch (error) {
            const handledError = ErrorHandler.handleDataverseError(error);
            results.push({
              operation: 'create',
              success: false,
              error: ErrorHandler.getUserFriendlyMessage(handledError),
              rollbackRequired: true
            });
          }
        }

        // Execute updates
        for (const record of operation.updates) {
          if (record.id) {
            try {
              await this.updateJunctionRecord(config, record.id, record.rowId, record.columnId);
              results.push({ operation: 'update', id: record.id, success: true });
            } catch (error) {
              const handledError = ErrorHandler.handleDataverseError(error);
              results.push({
                operation: 'update',
                id: record.id,
                success: false,
                error: ErrorHandler.getUserFriendlyMessage(handledError)
              });
            }
          }
        }

        // Execute deletes
        // Soft-delete (deactivate) instead of hard delete
        for (const junctionId of operation.deletes) {
          try {
            // Minimal change: perform update setting inactive status fields if present
            // Assumes standard Dataverse fields statecode/statuscode; adjust if entity uses custom
            const deactivatePayload: any = {
              statecode: 1, // Inactive
              statuscode: 2 // Common inactive status (may vary per entity)
            };
            await this.webAPI.updateRecord(config.junctionEntityName, junctionId, deactivatePayload);
            results.push({ operation: 'softdelete', id: junctionId, success: true });
          } catch (error) {
            const handledError = ErrorHandler.handleDataverseError(error);
            results.push({
              operation: 'softdelete',
              id: junctionId,
              success: false,
              error: ErrorHandler.getUserFriendlyMessage(handledError)
            });
          }
        }

        // Call API after batch operation
        if (config.entityName?.toLowerCase() === "ktr_managedlist") {
          console.log(
            "Detected entity: ktr_managedlist — post-batch logic start: entering API condition"
          );

          // Collect all successful soft deletes
          const deletedRecords = results.filter(
            (r) => r.operation === "softdelete" && r.success
          );

          if (deletedRecords.length > 0) {
            console.log(
              `Detected ${deletedRecords.length} successful soft delete(s) — calling API once with all IDs`
            );

            try {
              await this.handleStudyManagedListEntitySoftDelete(config, deletedRecords);
            } catch (err) {
              console.error("Error while processing managed list soft delete batch:", err);
            }
          } else {
            console.log(
              "Detected entity: ktr_managedlist — no soft deletes detected - skipping API call"
            );
          }
        }

        // Analyze results
        const successful = results.filter(r => r.success).length;
        const failed = results.filter(r => !r.success).length;
        const errors = results.filter(r => !r.success).map(r => r.error!);

        if (failed === 0) {
          // Invalidate relevant caches on successful save
          this.cacheManager.invalidateEntityCache(config.junctionEntityName);
          return { success: true };
        }

        // Partial success - decide whether to rollback
        const shouldRollback = results.some(r => r.rollbackRequired) && failed > successful;

        if (shouldRollback) {
          await this.rollbackCreatedRecords(config, createdIds);
          return {
            success: false,
            errors: ['Operation rolled back due to critical failures', ...errors]
          };
        }

        return {
          success: successful > 0,
          errors: errors.length > 0 ? errors : ['Some operations failed']
        };

      } catch (error) {
        // Critical error - attempt rollback
        if (createdIds.length > 0) {
          await this.rollbackCreatedRecords(config, createdIds);
        }

        const handledError = ErrorHandler.handleDataverseError(error);
        this.log('error', 'Batch save critical error', handledError);

        return {
          success: false,
          errors: [ErrorHandler.getUserFriendlyMessage(handledError)]
        };
      }
    });
  }

  /**
 * Handles custom logic for KTR Study Managed List Entity - unassignment
 * Prepares input parameters and calls ktr_validate_study_template API.
 */
  private async handleStudyManagedListEntitySoftDelete(
    config: MatrixConfig,
    deactivatedRecords: BatchOperationResult[]
  ): Promise<void> {
    try {
      const deletedIds = deactivatedRecords
        .map(r => r.id)
        .filter((id): id is string => !!id);

      if (deletedIds.length === 0) {
        console.warn("No valid IDs found for deactivated records, skipping API call.");
        return;
      }

      const request = {
        getMetadata: () => ({
          boundParameter: null,
          parameterTypes: {
            StudyManagedListEntityIds: {
              typeName: "Edm.String",
              structuralProperty: 1
            }
          },
          operationName: "ktr_validate_study_template",
          operationType: 0
        }),
        StudyManagedListEntityIds: JSON.stringify(deletedIds)
      };

      console.log("Calling first API (ktr_validate_study_template)...");

      const response = await (this.webAPI as any).execute(request);

      // Extract StudyIds from the response
      const result = await response.json();
      // Extract StudyIds
      let studyIds: string[] = [];
      try {
        studyIds = JSON.parse(result.StudyIds || "[]");
      } catch (parseErr) {
        console.error("Failed to parse StudyIds:", result.StudyIds, parseErr);
        return;
      }

      console.log("Received study IDs from API:", studyIds);

      // Call 2nd API using HTTP POST for each Study ID
      for (const studyId of studyIds) {
        try {
          console.log(`Calling ktr_detect_or_create_subset for Study: ${studyId}`);
          await this.createOrDetectSubsetAPI(studyId);
          console.log(`Successfully processed Study: ${studyId}`);
        } catch (err) {
          console.error(`Error processing Study ${studyId}:`, err);
        }
      }

      console.log("All study subset creation calls completed.");

    } catch (error) {
      const handledError = ErrorHandler.handleDataverseError(error);
      console.error("Error during managed list soft delete handling:", handledError);
    }
  }

  /**
     * Calls the ktr_detect_or_create_subset Custom API
     * @param studyId Guid of the study record
     */
  async createOrDetectSubsetAPI(studyId: string): Promise<void> {
    try {
      const actionName = "ktr_detect_or_create_subset";

      // Build the URL for the unbound Custom API
      let baseUrl = UrlHelper.getBaseUrl(this.context);
      const url = `${baseUrl}/${actionName}`;

      // Prepare the request body
      const body = { studyId };

      // Execute the HTTP POST request
      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Accept": "application/json",
          "Content-Type": "application/json; charset=utf-8",
          "OData-MaxVersion": "4.0",
          "OData-Version": "4.0"
        },
        body: JSON.stringify(body)
      });

      if (response.ok) {
        console.log(`ktr_detect_or_create_subset completed successfully for StudyId: ${studyId}`);
      } else {
        console.error("Error executing ktr_detect_or_create_subset API:", response.statusText);
      }
    } catch (error: any) {
      console.error("Error executing ktr_detect_or_create_subset API:", error);
      throw error;
    }
  }

  /**
   * Rollback created records in case of batch failure
   */
  private async rollbackCreatedRecords(config: MatrixConfig, createdIds: string[]): Promise<void> {
    this.log('warn', `Rolling back ${createdIds.length} created records`);

    const rollbackPromises = createdIds.map(async id => {
      try {
        await this.webAPI.deleteRecord(config.junctionEntityName, id);
        this.log('info', `Rollback successful for record ${id}`);
      } catch (rollbackError) {
        this.log('error', `Rollback failed for record ${id}`, rollbackError);
      }
    });

    await Promise.allSettled(rollbackPromises);
  }

  /**
   * Enhanced privilege checking with better caching and error detection
   */
  async checkEntityPrivileges(entityName: string, privilege: 'Create' | 'Read' | 'Write' | 'Delete'): Promise<boolean> {
    const cacheKey = `privileges:${entityName}:${privilege}`;

    return this.cacheManager.getOrFetch(cacheKey, async () => {
      try {
        // Use a minimal FetchXML query to test read access
        const fetchXml = FetchXMLQueryBuilder.buildCountQuery(entityName);
        await this.executeFetchXMLQuery<DataverseEntity>(entityName, fetchXml, 'PRIVILEGE CHECK');

        return true;

      } catch (error) {
        const handledError = ErrorHandler.handleDataverseError(error);
        const hasNoAccess = ErrorHandler.isPermissionError(handledError);

        this.log(hasNoAccess ? 'warn' : 'error', `Privilege check for ${entityName}:${privilege}`, {
          hasAccess: !hasNoAccess,
          error: handledError.message
        });

        return !hasNoAccess;
      }
    });
  }

  /**
   * Create a single junction record with enhanced error handling
   */
  async createJunctionRecord(config: MatrixConfig, rowId: string, columnId: string, sortOrder: number): Promise<string> {
    return this.performanceTracker.track('create_junction', async () => {
      // Minimal enhancement: before creating, check if an inactive junction already exists for this row-column
      const existing = await this.findExistingJunction(config, rowId, columnId);
      if (existing && existing.id) {
        // Reactivate instead of create
        try {
          const activatePayload: any = { statecode: 0, statuscode: 1 };
          await this.webAPI.updateRecord(config.junctionEntityName, existing.id, activatePayload);
          this.cacheManager.invalidateEntityCache(config.junctionEntityName);
          this.log('info', 'Reactivated existing inactive junction instead of creating new', { rowId, columnId, junctionId: existing.id });
          return existing.id;
        } catch (error) {
          const handledError = ErrorHandler.handleDataverseError(error);
          this.log('error', 'Failed to reactivate existing junction, falling back to create', handledError);
          // Proceed to create as fallback
        }
      }
      const rowSchemaName = this.getSchemaNameForSave(config.junctionEntityName, config.junctionRowField);
      const columnSchemaName = this.getSchemaNameForSave(config.junctionEntityName, config.junctionColumnField);

      const rowEntitySet = NamingConventionManager.getEntitySetNameForSave(config.rowEntityName);
      const columnEntitySet = NamingConventionManager.getEntitySetNameForSave(config.columnEntityName);

      let createData: any = {};
      const junctionName = config.junctionEntityName.toLowerCase();

      switch (junctionName) {
        case "ktr_studyquestionnaireline":
          // --- Question case ---
          createData = {
            "ktr_QuestionnaireLine@odata.bind": `kt_questionnairelineses(${rowId})`,
            "ktr_Study@odata.bind": `kt_studies(${columnId})`,
            ktr_sortorder: sortOrder ?? 0
          };
          break;

        case "ktr_studymanagedlistentity": {
          // --- Managed List case ---
          let answerText: string | null = null;
          try {
            const rowRecord = await this.webAPI.retrieveRecord(
              config.rowEntityName, // ktr_managedlistentity
              rowId,
              "?$select=ktr_answertext"
            );
            answerText = rowRecord?.ktr_answertext ?? null;
          } catch (e) {
            console.warn("Could not fetch answer text for managed list entity", e);
          }

          createData = {
            "ktr_ManagedListEntity@odata.bind": `ktr_managedlistentities(${rowId})`,
            "ktr_Study@odata.bind": `kt_studies(${columnId})`
          };

          // Set junction name from answer text
          if (answerText) {
            createData["ktr_name"] = answerText;
          }

          break;
        }

        default:
          console.warn("[createJunctionRecord] Unknown junction entity", config.junctionEntityName);
          break;
      }

      this.log("info", "Creating junction record", {
        rowSchemaName,
        columnSchemaName,
        rowEntitySet,
        columnEntitySet,
        createData
      });

      try {
        const result = await this.webAPI.createRecord(config.junctionEntityName, createData);
        console.log("[createJunctionRecord] Create record result:", result);
        this.cacheManager.invalidateEntityCache(config.junctionEntityName);
        return result.id;
      } catch (error) {
        console.error("[createJunctionRecord] Error caught:", error);
        const handledError = ErrorHandler.handleDataverseError(error);
        this.log("error", `Failed to create junction record ${rowId}-${columnId}`, handledError);
        throw handledError;
      }
    });
  }

  /**
   * Minimal helper: fetch existing junction (active or inactive) for a row-column pair.
   * Returns first matching record or undefined. Inactive ones are preferred for reactivation.
   */
  private async findExistingJunction(config: MatrixConfig, rowId: string, columnId: string): Promise<JunctionRecord | undefined> {
    try {
      const fetchXml = FetchXMLQueryBuilder.buildExistingJunctionQuery(config, rowId, columnId);
      const idField = this.getJunctionIdField(config);
      const response = await this.executeFetchXMLQuery<DataverseEntity>(config.junctionEntityName, fetchXml, 'CHECK EXISTING JUNCTION');
      if (!response.entities || response.entities.length === 0) return undefined;

      const entity = response.entities[0];
      return {
        id: entity[idField],
        rowId: rowId,
        columnId: columnId,
        statecode: entity.statecode,
        statuscode: entity.statuscode
      } as JunctionRecord;
    } catch (error) {
      // Silently ignore lookup failure; treat as no existing junction
      this.log('warn', 'Existing junction check failed (ignored)', { rowId, columnId });
      return undefined;
    }
  }

  /**
   * Update a single junction record
   */
  async updateJunctionRecord(config: MatrixConfig, junctionId: string, rowId: string, columnId: string): Promise<void> {
    return this.performanceTracker.track('update_junction', async () => {
      const rowSchemaName = this.getSchemaNameForSave(config.junctionEntityName, config.junctionRowField);
      const columnSchemaName = this.getSchemaNameForSave(config.junctionEntityName, config.junctionColumnField);

      const rowEntitySet = NamingConventionManager.getEntitySetNameForSave(config.rowEntityName);
      const columnEntitySet = NamingConventionManager.getEntitySetNameForSave(config.columnEntityName);

      const updateData = {
        [`${rowSchemaName}@odata.bind`]: `/${rowEntitySet}(${rowId})`,
        [`${columnSchemaName}@odata.bind`]: `/${columnEntitySet}(${columnId})`
      };

      try {
        await this.webAPI.updateRecord(config.junctionEntityName, junctionId, updateData);
        this.cacheManager.invalidateEntityCache(config.junctionEntityName);
      } catch (error) {
        const handledError = ErrorHandler.handleDataverseError(error);
        this.log('error', `Failed to update junction record ${junctionId}`, handledError);
        throw handledError;
      }
    });
  }

  /**
   * Delete a single junction record
   */
  async deleteJunctionRecord(config: MatrixConfig, junctionId: string): Promise<void> {
    return this.performanceTracker.track('delete_junction', async () => {
      try {
        await this.webAPI.deleteRecord(config.junctionEntityName, junctionId);
        this.cacheManager.invalidateEntityCache(config.junctionEntityName);
      } catch (error) {
        const handledError = ErrorHandler.handleDataverseError(error);
        this.log('error', `Failed to delete junction record ${junctionId}`, handledError);
        throw handledError;
      }
    });
  }

  // Convenience methods for loading more data
  async loadMoreRows(config: MatrixConfig, skip: number, take: number = 10, parentRecordId?: string): Promise<RowEntity[]> {
    const result = await this.loadEntities<RowEntity>(
      config.rowEntityName,
      config.rowIdField,
      config.rowDisplayField,
      config.rowParentField,
      skip,
      take,
      parentRecordId,
      'rows'
    );
    return result.entities;
  }

  async loadMoreColumns(config: MatrixConfig, skip: number, take: number = 10, parentRecordId?: string): Promise<ColumnEntity[]> {
    const result = await this.loadEntities<ColumnEntity>(
      config.columnEntityName,
      config.columnIdField,
      config.columnDisplayField,
      config.columnParentField,
      skip,
      take,
      parentRecordId,
      'columns'
    );
    return result.entities;
  }
  /**
   * Load all remaining columns from the specified skip position
   */
  async loadAllColumns(config: MatrixConfig, skip: number, parentRecordId?: string): Promise<ColumnEntity[]> {
    const allRemainingColumns: ColumnEntity[] = [];
    let currentSkip = skip;
    const batchSize = 50;

    try {
      // eslint-disable-next-line no-constant-condition
      while (true) {
        const batch = await this.loadEntities<ColumnEntity>(
          config.columnEntityName,
          config.columnIdField,
          config.columnDisplayField,
          config.columnParentField,
          currentSkip,
          batchSize,
          parentRecordId,
          'columns'
        );

        if (batch.entities.length === 0) {
          break; // No more entities to load
        }

        allRemainingColumns.push(...batch.entities);
        currentSkip += batch.entities.length;

        if (batch.entities.length < batchSize) {
          break;
        }

        this.log('info', `Loaded batch: ${batch.entities.length} columns, total so far: ${allRemainingColumns.length}`);
      }

      this.log('info', `Loaded all remaining columns: ${allRemainingColumns.length} total`);
      return allRemainingColumns;

    } catch (error: any) {
      const handledError = ErrorHandler.handleDataverseError(error);
      this.log('error', 'Failed to load all columns', handledError);
      throw handledError;
    }
  }
  /**
   * Get service statistics and performance metrics
   */
  getServiceStats(): {
    cache: ReturnType<CacheManager['getStats']>;
    performance: ReturnType<PerformanceTracker['getSummary']>;
    namingConvention: ReturnType<typeof NamingConventionManager.getCacheStats>;
    config: DataServiceConfig;
  } {
    return {
      cache: this.cacheManager.getStats(),
      performance: this.performanceTracker.getSummary(),
      namingConvention: NamingConventionManager.getCacheStats(),
      config: this.config
    };
  }

  /**
   * Log comprehensive service report
   */
  logServiceReport(): void {
    this.log('info', '=== DataService Performance Report ===');
    this.performanceTracker.logReport();

    const stats = this.getServiceStats();
    this.log('info', 'Cache Statistics', stats.cache);
    this.log('info', 'Naming Convention Cache', stats.namingConvention);
  }

  /**
   * Clear all caches and reset service state
   */
  public clearCache(): void {
    this.navigationPropertyCache.clear();
    this.cacheManager.clear();
    this.performanceTracker.reset();
    NamingConventionManager.clearCache();
    this.log('info', 'All caches cleared');
  }

  /**
   * Clean shutdown of the service
   */
  public destroy(): void {
    this.cacheManager.destroy();
    this.clearCache();
    this.log('info', 'DataService destroyed');
  }

  /**
   * Debug helper: Analyze junction entity field mapping
   * Call this method when field extraction is failing to diagnose the issue
   */
  public async diagnoseJunctionFieldMapping(config: MatrixConfig, sampleSize: number = 3): Promise<{
    entityName: string;
    sampleRecords: Array<{
      id: string;
      availableFields: string[];
      extractedRowId: string | undefined;
      extractedColumnId: string | undefined;
    }>;
    recommendations: string[];
  }> {

    this.log('info', 'Starting junction field mapping diagnosis...');

    try {
      const fetchXml = FetchXMLQueryBuilder.buildAllJunctionRecordsQuery(config, [], sampleSize);
      const response = await this.executeFetchXMLQuery<DataverseEntity>(
        config.junctionEntityName,
        fetchXml,
        'DIAGNOSE JUNCTION FIELDS'
      );

      const junctionIdField = NamingConventionManager.getPrimaryKeyField(config.junctionEntityName);
      const recommendations: string[] = [];

      const sampleRecords = response.entities.map((entity: DataverseEntity) => {
        const availableFields = Object.keys(entity);
        const extractedRowId = this.extractFieldValue(entity, config.junctionRowField);
        const extractedColumnId = this.extractFieldValue(entity, config.junctionColumnField);

        return {
          id: entity[junctionIdField],
          availableFields,
          extractedRowId,
          extractedColumnId
        };
      });

      // Generate recommendations
      if (sampleRecords.length === 0) {
        recommendations.push('No junction records found. Check if the junction entity name is correct.');
      } else {
        const allFields = [...new Set(sampleRecords.flatMap(r => r.availableFields))];

        // Check for missing row field mappings
        const failedRowExtractions = sampleRecords.filter(r => !r.extractedRowId).length;
        if (failedRowExtractions > 0) {
          const potentialRowFields = allFields.filter(f =>
            f.toLowerCase().includes(config.junctionRowField.toLowerCase()) ||
            f.toLowerCase().includes('row') ||
            f.endsWith('_value')
          );

          recommendations.push(
            `${failedRowExtractions}/${sampleRecords.length} records failed row ID extraction. ` +
            `Potential row fields: ${potentialRowFields.join(', ')}`
          );
        }

        // Check for missing column field mappings
        const failedColumnExtractions = sampleRecords.filter(r => !r.extractedColumnId).length;
        if (failedColumnExtractions > 0) {
          const potentialColumnFields = allFields.filter(f =>
            f.toLowerCase().includes(config.junctionColumnField.toLowerCase()) ||
            f.toLowerCase().includes('column') ||
            f.endsWith('_value')
          );

          recommendations.push(
            `${failedColumnExtractions}/${sampleRecords.length} records failed column ID extraction. ` +
            `Potential column fields: ${potentialColumnFields.join(', ')}`
          );
        }

        // Schema name recommendations
        if (failedRowExtractions > 0 || failedColumnExtractions > 0) {
          recommendations.push(
            'Consider using setSchemaNameOverride() to manually specify the correct field names.'
          );
        }

        if (recommendations.length === 0) {
          recommendations.push('Field mapping appears to be working correctly!');
        }
      }

      const diagnosis = {
        entityName: config.junctionEntityName,
        sampleRecords,
        recommendations
      };

      this.log('info', 'Junction field mapping diagnosis complete', diagnosis);

      return diagnosis;

    } catch (error) {
      const handledError = ErrorHandler.handleDataverseError(error);
      this.log('error', 'Junction field mapping diagnosis failed', handledError);
      throw handledError;
    }
  }
}