/**
 * DataServiceTypes.ts
 * 
 * This file defines types and interfaces for interacting with Dataverse entities, 
 * handling responses, errors, and caching mechanisms. It includes extended types for 
 * compatibility with PCF RetrieveMultipleResponse and performance tracking.
 */
import type { 
  MatrixConfig as BaseMatrixConfig, 
  RowEntity as BaseRowEntity, 
  ColumnEntity as BaseColumnEntity, 
  BatchOperation as BaseBatchOperation, 
  SaveResponse as BaseSaveResponse 
} from './MatrixTypes';


export interface DataverseEntity {
  [key: string]: any;
  createdon?: string;
}

export interface DataverseResponse {
  entities: DataverseEntity[];
  '@Microsoft.Dynamics.CRM.totalrecordcount'?: number;
  '@odata.count'?: number;
  '@odata.nextLink'?: string;
}

export interface FetchXMLResponse<T extends DataverseEntity = DataverseEntity> {
  entities: T[];
  '@Microsoft.Dynamics.CRM.totalrecordcount'?: number;
  '@odata.count'?: number;
  '@odata.nextLink'?: string;
}

export interface DataverseError {
  message: string;
  name: string;
  code?: string;
  statusCode?: number;
  innerError?: any;
  stack?: string;
}

export interface CacheEntry {
  data: any;
  timestamp: number;
  accessCount: number;
  lastAccessed: number;
}

export interface PerformanceStats {
  calls: number;
  totalTime: number;
  avgTime: number;
  minTime: number;
  maxTime: number;
  lastCall: number;
  errors: number;
}

export interface DataServiceConfig {
  cacheTTL?: number;
  maxRetries?: number;
  maxUrlLength?: number;
  debugMode?: boolean;
  enablePerformanceTracking?: boolean;
  cacheCleanupInterval?: number;
  maxCacheSize?: number;
}

/**
 * Extended interface for PCF RetrieveMultipleResponse with OData properties
 */
export interface ExtendedRetrieveMultipleResponse extends ComponentFramework.WebApi.RetrieveMultipleResponse {
  '@Microsoft.Dynamics.CRM.totalrecordcount'?: number;
  '@odata.count'?: number;
  '@odata.nextLink'?: string;
}

export interface LoadEntitiesResult<T> {
  entities: T[];
  totalCount: number;
  hasMore: boolean;
  nextPageInfo?: {
    skip: number;
    take: number;
  };
}

export interface BatchOperationResult {
  operation: 'create' | 'update' | 'delete' | 'softdelete';
  id?: string;
  success: boolean;
  error?: string;
  rollbackRequired?: boolean;
}

export interface SchemaValidationResult {
  isValid: boolean;
  validatedName: string;
  fallbackUsed: boolean;
  error?: string;
}

// Re-export base types with the same names for compatibility
export type MatrixConfig = BaseMatrixConfig;
export type RowEntity = BaseRowEntity;
export type ColumnEntity = BaseColumnEntity;
export type BatchOperation = BaseBatchOperation;
export type SaveResponse = BaseSaveResponse;

// Define enhanced JunctionRecord interface locally to avoid conflicts
export interface JunctionRecord {
  id: string;
  rowId: string;
  columnId: string;
  [key: string]: any;
}

export interface PartialJunctionRecord {
  id: string;
  rowId?: string;
  columnId?: string;
  [key: string]: any;
}