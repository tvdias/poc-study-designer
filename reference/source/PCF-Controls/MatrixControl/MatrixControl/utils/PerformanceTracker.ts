import { PerformanceStats } from '../types/DataServiceTypes';

export class PerformanceTracker {
  private stats = new Map<string, PerformanceStats>();
  private enabled: boolean;

  constructor(enabled: boolean = true) {
    this.enabled = enabled;
  }

  /**
   * Track performance of an operation
   */
  async track<T>(operation: string, fn: () => Promise<T>): Promise<T> {
    if (!this.enabled) {
      return fn();
    }

    const start = performance.now();
    let success = true;
    
    try {
      const result = await fn();
      return result;
    } catch (error) {
      success = false;
      throw error;
    } finally {
      const duration = performance.now() - start;
      this.recordOperation(operation, duration, success);
    }
  }

  /**
   * Track synchronous operation
   */
  trackSync<T>(operation: string, fn: () => T): T {
    if (!this.enabled) {
      return fn();
    }

    const start = performance.now();
    let success = true;
    
    try {
      const result = fn();
      return result;
    } catch (error) {
      success = false;
      throw error;
    } finally {
      const duration = performance.now() - start;
      this.recordOperation(operation, duration, success);
    }
  }

  /**
   * Record operation statistics
   */
  private recordOperation(operation: string, duration: number, success: boolean): void {
    const existing = this.stats.get(operation) || {
      calls: 0,
      totalTime: 0,
      avgTime: 0,
      minTime: Infinity,
      maxTime: 0,
      lastCall: 0,
      errors: 0
    };

    existing.calls++;
    existing.totalTime += duration;
    existing.avgTime = existing.totalTime / existing.calls;
    existing.minTime = Math.min(existing.minTime, duration);
    existing.maxTime = Math.max(existing.maxTime, duration);
    existing.lastCall = Date.now();
    
    if (!success) {
      existing.errors++;
    }

    this.stats.set(operation, existing);
  }

  /**
   * Get statistics for specific operation
   */
  getStats(operation: string): PerformanceStats | undefined {
    return this.stats.get(operation);
  }

  /**
   * Get all statistics
   */
  getAllStats(): Map<string, PerformanceStats> {
    return new Map(this.stats);
  }

  /**
   * Get performance summary
   */
  getSummary(): {
    operations: number;
    totalCalls: number;
    totalErrors: number;
    avgDuration: number;
    slowestOperation: { name: string; avgTime: number } | null;
  } {
    const operations = Array.from(this.stats.entries());
    
    if (operations.length === 0) {
      return {
        operations: 0,
        totalCalls: 0,
        totalErrors: 0,
        avgDuration: 0,
        slowestOperation: null
      };
    }

    const totalCalls = operations.reduce((sum, [, stats]) => sum + stats.calls, 0);
    const totalErrors = operations.reduce((sum, [, stats]) => sum + stats.errors, 0);
    const totalTime = operations.reduce((sum, [, stats]) => sum + stats.totalTime, 0);
    const avgDuration = totalTime / totalCalls;

    const slowestOperation = operations.reduce((slowest, [name, stats]) => {
      if (!slowest || stats.avgTime > slowest.avgTime) {
        return { name, avgTime: stats.avgTime };
      }
      return slowest;
    }, null as { name: string; avgTime: number } | null);

    return {
      operations: operations.length,
      totalCalls,
      totalErrors,
      avgDuration,
      slowestOperation
    };
  }

  /**
   * Reset all statistics
   */
  reset(): void {
    this.stats.clear();
  }

  /**
   * Enable/disable tracking
   */
  setEnabled(enabled: boolean): void {
    this.enabled = enabled;
  }

  /**
   * Log performance report to console
   */
  logReport(): void {
    if (!this.enabled || this.stats.size === 0) {
      console.log('Performance tracking disabled or no data available');
      return;
    }

    const summary = this.getSummary();
    console.group('DataService Performance Report');
    
    console.log(`Operations tracked: ${summary.operations}`);
    console.log(`Total calls: ${summary.totalCalls}`);
    console.log(`Total errors: ${summary.totalErrors} (${((summary.totalErrors / summary.totalCalls) * 100).toFixed(1)}%)`);
    console.log(`Average duration: ${summary.avgDuration.toFixed(2)}ms`);
    
    if (summary.slowestOperation) {
      console.log(`Slowest operation: ${summary.slowestOperation.name} (${summary.slowestOperation.avgTime.toFixed(2)}ms avg)`);
    }

    console.group('Detailed Stats:');
    for (const [operation, stats] of this.stats.entries()) {
      const errorRate = stats.calls > 0 ? (stats.errors / stats.calls * 100).toFixed(1) : '0';
      console.log(`${operation}:`, {
        calls: stats.calls,
        avgTime: `${stats.avgTime.toFixed(2)}ms`,
        minTime: `${stats.minTime.toFixed(2)}ms`,
        maxTime: `${stats.maxTime.toFixed(2)}ms`,
        errorRate: `${errorRate}%`,
        lastCall: new Date(stats.lastCall).toLocaleTimeString()
      });
    }
    console.groupEnd();
    console.groupEnd();
  }
}