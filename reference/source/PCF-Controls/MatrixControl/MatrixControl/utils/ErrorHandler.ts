import { DataverseError } from '../types/DataServiceTypes';

export class ErrorHandler {
  
  /**
   * Standardize error handling across the service
   */
  static handleDataverseError(error: unknown): DataverseError {
    if (this.isDataverseError(error)) {
      return error;
    }
    
    if (error instanceof Error) {
      return {
        message: error.message,
        name: error.name,
        code: (error as any).code,
        statusCode: (error as any).statusCode,
        innerError: (error as any).innerError,
        stack: error.stack
      };
    }
    
    return {
      message: `Unknown error: ${error}`,
      name: 'UnknownError'
    };
  }

  /**
   * Type guard for DataverseError
   */
  static isDataverseError(error: any): error is DataverseError {
    return error && typeof error === 'object' && 'message' in error && 'name' in error;
  }

  /**
   * Check if error indicates no retry should be attempted
   */
  static isNonRetryableError(error: DataverseError): boolean {
    const nonRetryableCodes = ['403', '401', '400'];
    const nonRetryableMessages = [
      'forbidden',
      'unauthorized', 
      'bad request',
      'url too long',
      'invalid entity',
      'insufficient privileges'
    ];

    // Check status code
    if (error.statusCode && nonRetryableCodes.includes(error.statusCode.toString())) {
      return true;
    }

    // Check error code
    if (error.code && nonRetryableCodes.includes(error.code)) {
      return true;
    }

    // Check message content
    const errorMessage = error.message.toLowerCase();
    return nonRetryableMessages.some(msg => errorMessage.includes(msg));
  }

  /**
   * Determine if error indicates permission/privilege issues
   */
  static isPermissionError(error: DataverseError): boolean {
    const permissionMessages = [
      '403',
      'forbidden',
      'insufficient privileges',
      'access denied',
      'unauthorized'
    ];

    const errorMessage = error.message.toLowerCase();
    return permissionMessages.some(msg => errorMessage.includes(msg)) ||
           error.statusCode === 403 ||
           error.code === '403';
  }

  /**
   * Extract user-friendly error message
   */
  static getUserFriendlyMessage(error: DataverseError): string {
    if (this.isPermissionError(error)) {
      return 'You do not have sufficient permissions to perform this operation.';
    }

    if (error.message.toLowerCase().includes('url too long')) {
      return 'Too much data to process at once. Please try with smaller data sets.';
    }

    if (error.message.toLowerCase().includes('network')) {
      return 'Network connection issue. Please check your connection and try again.';
    }

    if (error.message.toLowerCase().includes('timeout')) {
      return 'The operation took too long to complete. Please try again.';
    }

    // Return original message for other errors, but sanitized
    return this.sanitizeErrorMessage(error.message);
  }

  /**
   * Remove sensitive information from error messages
   */
  private static sanitizeErrorMessage(message: string): string {
    // Remove potential sensitive data patterns
    return message
      .replace(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/gi, '[ID]') // GUIDs
      .replace(/https?:\/\/[^\s]+/gi, '[URL]') // URLs
      .replace(/[\w.-]+@[\w.-]+\.\w+/gi, '[EMAIL]') // Email addresses
      .substring(0, 200); // Limit length
  }

  /**
   * Create aggregated error from multiple errors
   */
  static createAggregatedError(errors: DataverseError[], operation: string): DataverseError {
    const uniqueMessages = [...new Set(errors.map(e => this.getUserFriendlyMessage(e)))];
    const errorCounts = errors.reduce((acc, error) => {
      const msg = this.getUserFriendlyMessage(error);
      acc[msg] = (acc[msg] || 0) + 1;
      return acc;
    }, {} as Record<string, number>);

    const summary = uniqueMessages
      .map(msg => errorCounts[msg] > 1 ? `${msg} (${errorCounts[msg]} times)` : msg)
      .join('; ');

    return {
      message: `${operation} failed: ${summary}`,
      name: 'AggregatedDataverseError',
      code: 'BATCH_OPERATION_FAILED',
      innerError: { originalErrors: errors, errorCounts }
    };
  }
}