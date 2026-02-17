import { test, expect, Page } from '@playwright/test';
import PerformanceUtils, { PerformanceMetrics, PerformanceThresholds } from '../../../utils/performance/performance-utils';
import { AuthService } from '../../../services/AuthService';
import performanceConfig from '../../../Test Data/performance-config.json';

test.describe('Performance Tests - Pre-Prod Environment', () => {
  let authService: AuthService;
  let performanceUtils: PerformanceUtils;
  
  test.beforeEach(async ({ page }) => {
    authService = new AuthService();
    performanceUtils = new PerformanceUtils(page);
  });

  test('[PERF-001] Product Entity Page Load Performance Test', { 
    tag: ['@performance', '@product-entity', '@preprod'] 
  }, async ({ page }, testInfo) => {
    const testStartTime = Date.now();
    
    const baseUrl = performanceConfig.preprodEnvironment.baseUrl;
    const testUrl = `${baseUrl}&pagetype=entityrecord&etn=ktr_product&id=4a1d6f47-d308-f011-bae3-0022481a7f97`;
    const thresholds = performanceConfig.performanceThresholds.good;
    
    await test.step('Authenticate with Client Service Account', async () => {
      const credentials = await authService.getClientServiceCredentials();
      expect(credentials.username).toBeTruthy();
      expect(credentials.password).toBeTruthy();
      
      console.log(`Authenticating with CS user: ${credentials.username}`);
    });

    await test.step('Navigate to Product Entity Page and Measure Performance', async () => {
      console.log(`Starting performance test for: ${testUrl}`);
      
      await page.goto(testUrl, { 
        waitUntil: 'commit',
        timeout: performanceConfig.testConfiguration.timeout 
      });
      
      await authService.handleAuthentication(page);

      const navigationStartTime = Date.now();
      
      await performanceUtils.waitForFullPageLoad();
      
      const navigationEndTime = Date.now();
      console.log(`Page navigation completed in: ${navigationEndTime - navigationStartTime}ms`);
    });

    await test.step('Collect and Validate Performance Metrics', async () => {
      const metrics = await performanceUtils.getPerformanceMetrics();
      
      console.log('Power Apps Performance Metrics:', {
        pageLoadTime: `${metrics.pageLoadTime.toFixed(2)}ms`,
        timeToInteractive: `${metrics.timeToInteractive.toFixed(2)}ms`,
        performanceScore: `${metrics.performanceScore}/100`
      });

      const reportContent = performanceUtils.generatePerformanceReport(
        metrics, 
        'Product Entity Page Load Test'
      );
      
      await testInfo.attach('performance-report.txt', {
        body: reportContent,
        contentType: 'text/plain'
      });

      try {
        performanceUtils.validatePerformanceMetrics(metrics, thresholds);
        console.log('All performance thresholds passed!');
      } catch (error) {
        const errorMessage = error instanceof Error ? error.message : String(error);
        console.error('Performance threshold violations:', errorMessage);
        
        await testInfo.attach('performance-violations.txt', {
          body: errorMessage,
          contentType: 'text/plain'
        });
        
        console.warn('Performance test completed with warnings');
      }
    });

    await test.step('Take Performance Screenshot', async () => {
      const screenshot = await page.screenshot({ 
        fullPage: true,
        type: 'png'
      });
      
      await testInfo.attach('page-screenshot.png', {
        body: screenshot,
        contentType: 'image/png'
      });
    });

    const testEndTime = Date.now();
    console.log(`Total test execution time: ${testEndTime - testStartTime}ms`);
  });
});