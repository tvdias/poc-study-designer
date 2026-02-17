import { MatrixConfig } from '../types/DataServiceTypes';
import { NamingConventionManager } from './NamingConventionManager';

/**
 * Enhanced FetchXML Query Builder with security improvements and validation
 */
export class FetchXMLQueryBuilder {

  /**
   * Build secure FetchXML for row entities with enhanced filtering
   */
  /**
   * Builds a FetchXML query string to retrieve row entities based on the provided matrix configuration,
   * pagination parameters, parent record ID, and optional additional filters.
   *
   * @param config - The matrix configuration object containing entity and field information.
   * @param skip - The number of records to skip for pagination (default is 0).
   * @param take - The number of records to retrieve per page (default is 20).
   * @param parentRecordId - (Optional) The ID of the parent record to filter by.
   * @param additionalFilters - (Optional) An array of additional filter conditions to apply.
   * @returns The constructed FetchXML query string.
   */

  private static readonly studyAbandonStatus = 847610005;

  static buildRowEntitiesQuery(
    config: MatrixConfig,
    skip: number = 0,
    take: number = 20,
    parentRecordId?: string,
    additionalFilters?: Array<{ attribute: string; operator: string; value: string }>
    ): string {

    const rowIdField = config.rowIdField || NamingConventionManager.getPrimaryKeyField(config.rowEntityName);
    const pageNumber = Math.floor(skip / take) + 1;

    let filterConditions = '';
    const conditions: string[] = [];

    // Always include active records
    conditions.push(`<condition attribute="statecode" operator="eq" value="0" />`);

    // Parent record filter
    if (parentRecordId && config.rowParentField) {
      const parentField = NamingConventionManager.getFetchXMLFieldName(config.rowParentField);
      const safeParentId = NamingConventionManager.escapeXmlValue(parentRecordId);
      conditions.push(`<condition attribute="${parentField}" operator="eq" value="${safeParentId}" />`);
    }

    // Additional filters
    if (additionalFilters && additionalFilters.length > 0) {
      for (const filter of additionalFilters) {
        const safeValue = NamingConventionManager.escapeXmlValue(filter.value);
        conditions.push(`<condition attribute="${filter.attribute}" operator="${filter.operator}" value="${safeValue}" />`);
      }
    }

    filterConditions = `
      <filter type="and">
        ${conditions.join('\n      ')}
      </filter>`;

    // Sort only for Questions (not for Managed List Entities)
    const isManagedListEntity = config.rowEntityName?.toLowerCase() === 'ktr_managedlistentity';
    const sortAttributes = isManagedListEntity ? '' : `
      <attribute name="kt_questionsortorder" />
      <order attribute="kt_questionsortorder" descending="false" />`;

    const fetchXml = `
      <fetch page="${pageNumber}" count="${take}">
        <entity name="${config.rowEntityName}">
          <attribute name="${rowIdField}" />
          <attribute name="${config.rowDisplayField}" />
          <attribute name="createdon" />
          ${sortAttributes}
          ${filterConditions}
        </entity>
      </fetch>
    `.trim();

    return fetchXml;
  }

  static buildColumnCountQuery(
    config: MatrixConfig,
    parentRecordId?: string
  ): string {
    const conditions: string[] = [];

    // Always active studies
    conditions.push(`<condition attribute="statecode" operator="eq" value="0" />`);
    // Exclude abandoned ones
    conditions.push(`<condition attribute="statuscode" operator="neq" value="${this.studyAbandonStatus}" />`);

    // Determine if it's a Managed List form
    const resolvedParentEntityName = (config.parentEntityName || config.entityName || '').toLowerCase();

    const isEntityToProjectStudy =
    parentRecordId != null &&
    (resolvedParentEntityName === 'ktr_managedlist');

    let fetchXml: string;

    if (isEntityToProjectStudy  && parentRecordId) {
        fetchXml = this.buildEntityToProjectStudyFetchXml(
        config,
        0, // not paginated
        1, // dummy page number
        parentRecordId,
        [
          `<attribute name="${NamingConventionManager.getFetchXMLFieldName(config.columnIdField)}" aggregate="count" alias="totalcount" />`
        ],
        `
        <filter type="and">
          ${conditions.join('\n        ')}
        </filter>`,
        true // aggregate
    );
    }
    else {
      // Default case (e.g., Project form)
      const parentFilter =
        parentRecordId && config.columnParentField
          ? `<condition attribute="${NamingConventionManager.getFetchXMLFieldName(config.columnParentField)}" operator="eq" value="${NamingConventionManager.escapeXmlValue(parentRecordId)}" />`
          : '';

      fetchXml = `
        <fetch aggregate="true">
          <entity name="${config.columnEntityName}">
            <attribute name="createdon" aggregate="count" alias="totalcount" />
            <filter type="and">
              ${conditions.join('\n            ')}
              ${parentFilter}
            </filter>
          </entity>
        </fetch>`.trim();   
    }
    console.log('[buildColumnCountQuery] Final FetchXML:', fetchXml);

    return fetchXml;
  }

  /**
   * Build secure FetchXML for column entities with enhanced filtering
   */
  static buildColumnEntitiesQuery(
    config: MatrixConfig,
    skip: number = 0,
    take: number = 20,
    parentRecordId?: string,
    additionalFilters?: Array<{ attribute: string; operator: string; value: string }>,
    includeVersionFields: boolean = true
  ): string {

    const columnIdField = config.columnIdField || NamingConventionManager.getPrimaryKeyField(config.columnEntityName);
    const pageNumber = Math.floor(skip / take) + 1;

    const conditions: string[] = [];

    // Always exclude abandoned studies
    conditions.push(`<condition attribute="statuscode" operator="neq" value="${this.studyAbandonStatus}" />`);
    // Only active studies
    conditions.push(`<condition attribute="statecode" operator="eq" value="0" />`);

    // Add any extra filters (if present)
    if (additionalFilters && additionalFilters.length > 0) {
      for (const filter of additionalFilters) {
        const safeValue = NamingConventionManager.escapeXmlValue(filter.value);
        conditions.push(`<condition attribute="${filter.attribute}" operator="${filter.operator}" value="${safeValue}" />`);
      }
    }

    let filterConditions = `
      <filter type="and">
        ${conditions.join('\n      ')}
      </filter>`;

     // === CASE: Special handling for entity -> project -> study pattern ===
    const resolvedParentEntityName = (config.parentEntityName || config.entityName || '').toLowerCase();

    // isEntityToProjectStudy only true if we have a parentRecordId and entity names match the pattern
    const isEntityToProjectStudy = parentRecordId != null &&
    (resolvedParentEntityName === 'ktr_managedlist');

    // Build common attributes
    const attributes = [
      `<attribute name="${columnIdField}" />`,
      `<attribute name="${config.columnDisplayField}" />`,
      `<attribute name="createdon" />`,
      `<attribute name="statuscode" />`
    ];

    if (config.columnParentAttrField) {
      const parentAttrField = NamingConventionManager.getFetchXMLFieldName(config.columnParentAttrField);
      attributes.push(`<attribute name="${parentAttrField}" />`);
    }

    if (config.columnVersionField) {
      const versionField = NamingConventionManager.getFetchXMLFieldName(config.columnVersionField);
      attributes.push(`<attribute name="${versionField}" />`);
    }

    let fetchXml: string;

    if (isEntityToProjectStudy  && parentRecordId) 
    {
      fetchXml = this.buildEntityToProjectStudyFetchXml(config, take, pageNumber, parentRecordId, attributes, filterConditions);
    } 
    else 
    {
      // Normal case (Questionnaire / default behavior)
      const parentField = config.columnParentField
        ? NamingConventionManager.getFetchXMLFieldName(config.columnParentField)
        : undefined;

      const parentFilter = parentField && parentRecordId
        ? `<condition attribute="${parentField}" operator="eq" value="${NamingConventionManager.escapeXmlValue(parentRecordId)}" />`
        : "";

      fetchXml = `
        <fetch count="${take}" page="${pageNumber}" returntotalrecordcount="true">
          <entity name="${config.columnEntityName}">
            ${attributes.join('\n          ')}
            <order attribute="createdon" descending="true" />
            <filter type="and">
              ${conditions.join('\n            ')}
              ${parentFilter}
            </filter>
          </entity>
        </fetch>`.trim();
    }
    return fetchXml;
  }

  /**
 * Helper method for patterns like:
 *   [Entity] → Project → Study
 * Example: Managed List → Project → Study
 */
  private static buildEntityToProjectStudyFetchXml(
    config: MatrixConfig,
    take: number,
    pageNumber: number,
    parentRecordId: string,
    attributes: string[],
    filterConditions: string,
    isAggregate: boolean = false
    ): string {
    const fetchAttrs = isAggregate
      ? `aggregate="true"`
      : `count="${take}" page="${pageNumber}" returntotalrecordcount="true"`;

    return `
      <fetch ${fetchAttrs}>
        <entity name="${config.columnEntityName}">
          ${attributes.join('\n        ')}
          ${isAggregate ? '' : '<order attribute="createdon" descending="true" />'}
          ${filterConditions}

          <!-- Generic entity → Project → Study pattern -->
          <link-entity name="${config.parentEntityName}" from="ktr_project" to="${config.columnParentField}" link-type="inner">
            <filter type="and">
              <condition attribute="${config.parentEntityId}" operator="eq" value="${NamingConventionManager.escapeXmlValue(parentRecordId)}" />
            </filter>
          </link-entity>
        </entity>
      </fetch>`.trim();
  }

  /**
   * This is used by DataService to automatically detect when to enhance queries
   */
  static shouldIncludeVersionFields(config: MatrixConfig): boolean {
    return !!(config.columnParentAttrField || config.columnVersionField);
  }

  /**
   * Build secure FetchXML for junction records with batch size validation
   */
  static buildJunctionRecordsQuery(
    config: MatrixConfig,
    rowIds: string[],
    columnIds: string[]
  ): string {

    // Validate input arrays
    if (!rowIds.length || !columnIds.length) {
      throw new Error('Cannot build junction query with empty ID arrays');
    }

    // Validate batch size to prevent URL length issues
    const estimatedUrlLength = this.estimateJunctionQueryUrlLength(config, rowIds, columnIds);
    if (estimatedUrlLength > 4000) {
      throw new Error(`Junction query would be too long (${estimatedUrlLength} chars). Reduce batch size.`);
    }

    const junctionIdField = NamingConventionManager.getPrimaryKeyField(config.junctionEntityName);
    const junctionRowField = NamingConventionManager.getFetchXMLFieldName(config.junctionRowField);
    const junctionColumnField = NamingConventionManager.getFetchXMLFieldName(config.junctionColumnField);

    // Escape all IDs for security
    const safeRowIds = rowIds.map(id => this.validateAndEscapeGuid(id));
    const safeColumnIds = columnIds.map(id => this.validateAndEscapeGuid(id));

    const fetchXml = `
      <fetch>
        <entity name="${config.junctionEntityName}">
          <attribute name="${junctionIdField}" />
          <attribute name="${junctionRowField}" />
          <attribute name="${junctionColumnField}" />
          <filter type="and">
            <condition attribute="statuscode" operator="eq" value="1" />
            <condition attribute="${junctionRowField}" operator="in">
              ${safeRowIds.map(id => `<value>${id}</value>`).join('')}
            </condition>
            <condition attribute="${junctionColumnField}" operator="in">
              ${safeColumnIds.map(id => `<value>${id}</value>`).join('')}
            </condition>
          </filter>
        </entity>
      </fetch>`.trim();

    return fetchXml;
  }

  /**
   * Build FetchXML for all junction records with optional filtering
   */
  static buildAllJunctionRecordsQuery(
    config: MatrixConfig,
    columnIds: string[],
    maxRecords: number = 5000
  ): string {
    
    // Validate input arrays
    if (!columnIds.length) {
      throw new Error('Cannot build junction query with empty ID arrays');
    }

    const junctionIdField = NamingConventionManager.getPrimaryKeyField(config.junctionEntityName);
    const junctionRowField = NamingConventionManager.getFetchXMLFieldName(config.junctionRowField);
    const junctionColumnField = NamingConventionManager.getFetchXMLFieldName(config.junctionColumnField);

    const safeColumnIds = columnIds.map(id => this.validateAndEscapeGuid(id));

    const fetchXml = `
      <fetch count="${maxRecords}">
        <entity name="${config.junctionEntityName}">
          <attribute name="${junctionIdField}" />
          <attribute name="${junctionRowField}" />
          <attribute name="${junctionColumnField}" />
          <filter type="and">
            <condition attribute="statuscode" operator="eq" value="1" />
            <condition attribute="${junctionColumnField}" operator="in">
                ${safeColumnIds.map(id => `<value>${id}</value>`).join('')}
            </condition>
          </filter>
          <order attribute="createdon" descending="true" />
        </entity>
      </fetch>`.trim();

    return fetchXml;
  }

  /**
   * Calculate accurate URL length for validation
   */
  static calculateActualUrlLength(entityName: string, fetchXml: string): number {
    const baseUrl = `${entityName}?fetchXml=`;
    const encodedFetchXml = encodeURIComponent(fetchXml);
    return baseUrl.length + encodedFetchXml.length;
  }

  /**
   * Estimate junction query URL length before building
   */
  private static estimateJunctionQueryUrlLength(config: MatrixConfig, rowIds: string[], columnIds: string[]): number {
    const baseStructure = `<fetch><entity name="${config.junctionEntityName}"><attribute name=""/><filter type="and"><condition attribute="${config.junctionRowField}" operator="in"></condition><condition attribute="${config.junctionColumnField}" operator="in"></condition></filter></entity></fetch>`;

    // Estimate GUID values (36 chars each) plus XML overhead
    const valueOverhead = '<value></value>'.length;
    const rowValuesLength = rowIds.length * (36 + valueOverhead);
    const columnValuesLength = columnIds.length * (36 + valueOverhead);

    const totalXmlLength = baseStructure.length + rowValuesLength + columnValuesLength;

    // Account for URL encoding (approximately 3x for special characters)
    const estimatedEncodedLength = Math.ceil(totalXmlLength * 1.5);

    // Add base URL
    return `${config.junctionEntityName}?fetchXml=`.length + estimatedEncodedLength;
  }

  /**
   * Validate and escape GUID values
   */
  private static validateAndEscapeGuid(guid: string): string {
    if (!guid) {
      throw new Error('Empty GUID value');
    }

    // Basic GUID format validation (flexible to handle with/without braces)
    const guidPattern = /^(\{)?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}(\})?$/;

    if (!guidPattern.test(guid)) {
      throw new Error(`Invalid GUID format: ${guid}`);
    }

    // Remove braces if present and escape
    const cleanGuid = guid.replace(/[{}]/g, '');
    return NamingConventionManager.escapeXmlValue(cleanGuid);
  }

  /**
   * Build count-only query for performance
   */
  static buildCountQuery(entityName: string, parentRecordId?: string, parentField?: string): string {
    let filterConditions = '';

    if (parentRecordId && parentField) {
      const safeParentId = NamingConventionManager.escapeXmlValue(parentRecordId);
      filterConditions = `
        <filter type="and">
          <condition attribute="${parentField}" operator="eq" value="${safeParentId}" />
        </filter>`;
    }

    return `
      <fetch aggregate="true">
        <entity name="${entityName}">
          <attribute name="createdon" aggregate="count" alias="totalcount" />
          ${filterConditions}
        </entity>
      </fetch>`.trim();
  }

  /**
   * Validate FetchXML before execution
   */
  static validateFetchXML(fetchXml: string): { isValid: boolean; errors: string[] } {
    const errors: string[] = [];

    // Basic XML structure validation
    if (!fetchXml.includes('<fetch') || !fetchXml.includes('</fetch>')) {
      errors.push('Invalid FetchXML structure: missing fetch element');
    }

    if (!fetchXml.includes('<entity') || !fetchXml.includes('</entity>')) {
      errors.push('Invalid FetchXML structure: missing entity element');
    }

    // Check for potential injection patterns
    const dangerousPatterns = [
      /<script/i,
      /javascript:/i,
      /vbscript:/i,
      /onload/i,
      /onerror/i
    ];

    for (const pattern of dangerousPatterns) {
      if (pattern.test(fetchXml)) {
        errors.push('Potentially dangerous content detected in FetchXML');
        break;
      }
    }

    return {
      isValid: errors.length === 0,
      errors
    };
  }

  /**
   * Build FetchXML to find an existing junction record for a single rowId/columnId pair.
   * Includes state/status for reactivation decisions. Uses top="1".
   */
  static buildExistingJunctionQuery(config: MatrixConfig, rowId: string, columnId: string): string {
    const junctionRowField = NamingConventionManager.getFetchXMLFieldName(config.junctionRowField);
    const junctionColumnField = NamingConventionManager.getFetchXMLFieldName(config.junctionColumnField);
    const idField = config.junctionIdField || NamingConventionManager.getPrimaryKeyField(config.junctionEntityName);
    const safeRowId = NamingConventionManager.escapeXmlValue(rowId);
    const safeColumnId = NamingConventionManager.escapeXmlValue(columnId);

    return `
      <fetch top="1">
        <entity name="${config.junctionEntityName}">
          <attribute name="${idField}" />
          <attribute name="statecode" />
          <attribute name="statuscode" />
          <filter type="and">
            <condition attribute="${junctionRowField}" operator="eq" value="${safeRowId}" />
            <condition attribute="${junctionColumnField}" operator="eq" value="${safeColumnId}" />
          </filter>
        </entity>
      </fetch>
    `.trim();
  }
}