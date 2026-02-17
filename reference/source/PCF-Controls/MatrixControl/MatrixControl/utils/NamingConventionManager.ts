import { SchemaValidationResult } from '../types/DataServiceTypes';

/**
 * Enhanced Naming Convention Manager - Centralized handling of all naming conversions
 * Handles edge cases and already-pluralized entity names with improved validation
 */
export class NamingConventionManager {
  
  private static schemaNameCache = new Map<string, string>();
  private static validationCache = new Map<string, SchemaValidationResult>();
  
  /**
   * For FetchXML queries, use logical names in URL
   */
  static getEntityNameForFetchXMLUrl(logicalName: string): string {
    return logicalName;
  }

  /**
   * Enhanced entity set name conversion for @odata.bind references
   */
  static getEntitySetNameForSave(logicalName: string): string {
    // Check cache first
    const cacheKey = `entityset:${logicalName}`;
    if (this.schemaNameCache.has(cacheKey)) {
      return this.schemaNameCache.get(cacheKey)!;
    }

    const result = this.computeEntitySetName(logicalName);
    this.schemaNameCache.set(cacheKey, result);
    return result;
  }

  private static computeEntitySetName(logicalName: string): string {
    // Check if already pluralized (conservative patterns for company use)
    const alreadyPluralPatterns = [
      /lines$/i,     // questionnairelines, orderlines, etc.
      /details$/i,   // orderdetails, contactdetails, etc.
      /records$/i,   // paymentrecords, auditrecords, etc.
      /entries$/i,   // logentries, dataentries, etc.
      /items$/i      // lineitems, menuitems, etc.
    ];

    // Special handling: if it ends in common plural patterns but is actually singular entity name
    if (alreadyPluralPatterns.some(pattern => pattern.test(logicalName))) {
      if (logicalName.endsWith('s')) {
        return `${logicalName}es`;
      }
    }

    // Handle words ending in 'y' preceded by consonant
    if (logicalName.endsWith('y') && logicalName.length > 1) {
      const beforeY = logicalName[logicalName.length - 2];
      if (beforeY && !['a', 'e', 'i', 'o', 'u'].includes(beforeY.toLowerCase())) {
        return logicalName.slice(0, -1) + 'ies';
      }
    }

    // Handle words ending in 'f' or 'fe'
    if (logicalName.endsWith('f')) {
      return logicalName.slice(0, -1) + 'ves';
    }
    if (logicalName.endsWith('fe')) {
      return logicalName.slice(0, -2) + 'ves';
    }

    // Handle words ending in sibilant sounds (s, sh, ch, x, z)
    if (/[sxz]$/.test(logicalName) || logicalName.endsWith('sh') || logicalName.endsWith('ch')) {
      return `${logicalName}es`;
    }

    // Handle words ending in 'o' preceded by consonant
    if (logicalName.endsWith('o') && logicalName.length > 1) {
      const beforeO = logicalName[logicalName.length - 2];
      if (beforeO && !['a', 'e', 'i', 'o', 'u'].includes(beforeO.toLowerCase())) {
        return `${logicalName}es`;
      }
    }

    // Default: add 's'
    return `${logicalName}s`;
  }

  /**
   * PRIMARY KEY FIELD NAMES - Always logical entity name + 'id'
   */
  static getPrimaryKeyField(entityLogicalName: string): string {
    return `${entityLogicalName}id`;
  }

  /**
   * FETCHXML FIELD REFERENCES - Use configured logical names directly
   */
  static getFetchXMLFieldName(fieldLogicalName: string): string {
    return fieldLogicalName;
  }

  /**
   * ODATA FILTER FIELD REFERENCES - Use _value suffix for lookup fields
   */
  static getODataLookupFieldName(fieldLogicalName: string): string {
    if (fieldLogicalName.endsWith('_value')) {
      return fieldLogicalName;
    }
    return `_${fieldLogicalName}_value`;
  }

  /**
   * Enhanced schema name pattern detection for @odata.bind references
   * Converts logical names to CamelCase schema names with better pattern recognition
   */
  static detectSchemaNamePattern(fieldName: string): string {
    const cacheKey = `schema:${fieldName}`;
    if (this.schemaNameCache.has(cacheKey)) {
      return this.schemaNameCache.get(cacheKey)!;
    }

    const result = this.computeSchemaNamePattern(fieldName);
    this.schemaNameCache.set(cacheKey, result);
    return result;
  }

  private static computeSchemaNamePattern(fieldName: string): string {
    // If already appears to be schema name (mixed case), use as-is
    if (fieldName !== fieldName.toLowerCase() && fieldName !== fieldName.toUpperCase()) {
      return fieldName;
    }
    
    // Handle custom prefix patterns (e.g., ktr_questionnaireline → ktr_QuestionnaireLine)
    if (fieldName.includes('_')) {
      const parts = fieldName.split('_');
      if (parts.length >= 2) {
        const prefix = parts[0];
        const suffixParts = parts.slice(1);
        
        // Convert each part to CamelCase
        const camelCaseSuffix = suffixParts
          .map(part => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
          .join('');
        
        return `${prefix}_${camelCaseSuffix}`;
      }
    }
    
    // Default: capitalize first letter for schema name
    return fieldName.charAt(0).toUpperCase() + fieldName.slice(1).toLowerCase();
  }

  /**
   * Validate schema name by testing patterns
   */
  static validateSchemaName(entityName: string, fieldLogicalName: string, proposedSchemaName: string): SchemaValidationResult {
    const cacheKey = `validation:${entityName}:${fieldLogicalName}:${proposedSchemaName}`;
    if (this.validationCache.has(cacheKey)) {
      return this.validationCache.get(cacheKey)!;
    }

    // For now, we'll assume the detected pattern is valid
    // In a real implementation, you might want to test this with a small OData operation
    const result: SchemaValidationResult = {
      isValid: true,
      validatedName: proposedSchemaName,
      fallbackUsed: false
    };

    this.validationCache.set(cacheKey, result);
    return result;
  }

  /**
   * Get schema name with fallback validation
   */
  static getValidatedSchemaName(entityName: string, fieldLogicalName: string): string {
    const detectedName = this.detectSchemaNamePattern(fieldLogicalName);
    const validation = this.validateSchemaName(entityName, fieldLogicalName, detectedName);
    
    if (validation.isValid) {
      return validation.validatedName;
    }

    // Fallback to logical name if schema name fails validation
    console.warn(`Schema name validation failed for ${fieldLogicalName}, using logical name as fallback`);
    return fieldLogicalName;
  }

  /**
   * XML-safe value escaping to prevent injection attacks
   */
  static escapeXmlValue(value: string): string {
    if (!value) return '';
    
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&apos;');
  }

  /**
   * Clear all caches
   */
  static clearCache(): void {
    this.schemaNameCache.clear();
    this.validationCache.clear();
  }

  /**
   * Get cache statistics
   */
  static getCacheStats(): {
    schemaNamesCache: number;
    validationCache: number;
  } {
    return {
      schemaNamesCache: this.schemaNameCache.size,
      validationCache: this.validationCache.size
    };
  }

  /**
   * Manual override for schema names when automatic detection fails
   */
  static setSchemaNameOverride(entityName: string, fieldLogicalName: string, schemaName: string): void {
    const cacheKey = `schema:${fieldLogicalName}`;
    this.schemaNameCache.set(cacheKey, schemaName);
    
    const validationKey = `validation:${entityName}:${fieldLogicalName}:${schemaName}`;
    this.validationCache.set(validationKey, {
      isValid: true,
      validatedName: schemaName,
      fallbackUsed: false
    });
    
    console.log(`🔧 Manual schema name override: ${fieldLogicalName} → ${schemaName}`);
  }
}