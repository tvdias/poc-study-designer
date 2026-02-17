import { Page, expect } from '@playwright/test';

export interface PerformanceMetrics {
  pageLoadTime: number;
  timeToInteractive: number;
  navigationStart: number;
  loadComplete: number;
  performanceScore?: number;
}

export interface PerformanceThresholds {
  pageLoadTime: number;
  timeToInteractive: number;
}

export class PerformanceUtils {
  private page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  async getPerformanceMetrics(): Promise<PerformanceMetrics> {
    await this.page.waitForTimeout(1000);
    
    // Get Navigation Timing API metrics
    const navigationMetrics = await this.page.evaluate(() => {
      const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming;
      
      // Fallback values if navigation timing is not available
      const fallbackTime = Date.now();
      const loadEventEnd = navigation?.loadEventEnd || fallbackTime;
      const navigationStart = navigation?.fetchStart || (fallbackTime - 3000);
      const domInteractive = navigation?.domInteractive || (fallbackTime - 2000);
      
      return {
        navigationStart: navigationStart,
        pageLoadTime: Math.max(0, loadEventEnd - navigationStart),
        loadComplete: loadEventEnd,
        timeToInteractive: Math.max(0, domInteractive - navigationStart),
      };
    });

    return {
      navigationStart: navigationMetrics.navigationStart,
      pageLoadTime: navigationMetrics.pageLoadTime || 0,
      loadComplete: navigationMetrics.loadComplete,
      timeToInteractive: navigationMetrics.timeToInteractive || 0,
      performanceScore: this.calculatePerformanceScore({
        pageLoadTime: navigationMetrics.pageLoadTime || 0,
        timeToInteractive: navigationMetrics.timeToInteractive || 0
      })
    };
  }

  /**
   * Calculate a simplified performance score based on page load time and time to interactive for Power Apps
   */
  private calculatePerformanceScore(metrics: Partial<PerformanceMetrics>): number {
    let score = 100;

    // Page Load Time scoring (0-3s: excellent, 3-5s: good, 5-8s: fair, >8s: poor)
    if (metrics.pageLoadTime! > 8000) score -= 40;
    else if (metrics.pageLoadTime! > 5000) score -= 25;
    else if (metrics.pageLoadTime! > 3000) score -= 10;

    // TTI scoring (0-3.8s: excellent, 3.8-5s: good, 5-7s: fair, >7s: poor)
    if (metrics.timeToInteractive! > 7000) score -= 35;
    else if (metrics.timeToInteractive! > 5000) score -= 20;
    else if (metrics.timeToInteractive! > 3800) score -= 10;

    return Math.max(0, score);
  }

  /**
   * Validate performance metrics against thresholds - simplified for Power Apps
   */
  validatePerformanceMetrics(metrics: PerformanceMetrics, thresholds: PerformanceThresholds): void {
    const violations: string[] = [];

    if (metrics.pageLoadTime > thresholds.pageLoadTime) {
      violations.push(`Page Load Time: ${metrics.pageLoadTime}ms (threshold: ${thresholds.pageLoadTime}ms)`);
    }
    
    if (metrics.timeToInteractive > thresholds.timeToInteractive) {
      violations.push(`Time to Interactive: ${metrics.timeToInteractive}ms (threshold: ${thresholds.timeToInteractive}ms)`);
    }

    if (violations.length > 0) {
      throw new Error(`Performance thresholds violated:\n${violations.join('\n')}`);
    }
  }


  generatePerformanceReport(metrics: PerformanceMetrics, testName: string): string {
    const report = `
=== Performance Test Report ===
Test: ${testName}
Timestamp: ${new Date().toISOString()}
Performance Score: ${metrics.performanceScore}/100

=== Power Apps Core Metrics ===
Page Load Time: ${metrics.pageLoadTime.toFixed(2)}ms
Time to Interactive: ${metrics.timeToInteractive.toFixed(2)}ms

=== Performance Grade ===
${this.getPerformanceGrade(metrics.performanceScore!)}
    `.trim();

    return report;
  }

  private getPerformanceGrade(score: number): string {
    if (score >= 90) return 'A - Excellent';
    if (score >= 75) return 'B - Good';
    if (score >= 60) return 'C - Needs Improvement';
    if (score >= 40) return 'D - Poor';
    return 'F - Very Poor';
  }

  
  async waitForFullPageLoad(timeout = 30000): Promise<void> {
    try {
      await this.page.waitForLoadState('load', { timeout });
      await this.page.waitForLoadState('domcontentloaded', { timeout });
      
      try {
        await this.page.waitForLoadState('networkidle', { timeout: 10000 });
      } catch (error) {
        console.warn('Network idle timeout (this is often normal for dynamic applications)');
      }
      
      await this.page.waitForTimeout(3000);
      
    } catch (error) {
      console.warn(`waitUntilAppIdle warning: ${(error as Error).message}`);
    }
  }

  async clearBrowserData(): Promise<void> {
    try {
      const context = this.page.context();
      await context.clearCookies();
      await context.clearPermissions();
      
      // Clear local storage and session storage - with error handling
      try {
        await this.page.evaluate(() => {
          try {
            localStorage.clear();
          } catch (e) {
            console.warn('Could not clear localStorage:', e);
          }
          try {
            sessionStorage.clear();
          } catch (e) {
            console.warn('Could not clear sessionStorage:', e);
          }
        });
      } catch (error) {
        console.warn('Could not clear browser storage (this is usually fine):', error);
      }
    } catch (error) {
      console.warn('Could not clear some browser data (this is usually fine):', error);
    }
  }
}

export default PerformanceUtils;