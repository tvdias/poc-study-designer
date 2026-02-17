# Power Apps Performance Testing Guide

## Overview
This directory contains streamlined performance testing utilities for Power Apps applications, specifically focused on the essential metrics that matter most for Power Apps performance: **Page Load Time** and **Time to Interactive**.

## Files Structure

### Core Files
- **`page-load-performance.spec.ts`** - Main performance test for Power Apps
- **`performance-utils.ts`** - Simplified performance measurement utilities  
- **`performance-config.json`** - Configuration for thresholds and test settings

### Test Coverage
- **Product Entity Page Load Test** - Tests your specific Power Apps URL with authentication

## Performance Metrics (Simplified for Power Apps)

### Essential Metrics
- **🚀 Page Load Time** - Total time to fully load the Power Apps page
- **⚡ Time to Interactive** - When the page becomes fully interactive for users
- **📊 Performance Score** - Calculated overall score (0-100) based on the above metrics

## Performance Thresholds (Power Apps Optimized)

### Excellent Performance
- Page Load Time: < 2000ms (2 seconds)
- Time to Interactive: < 2500ms (2.5 seconds)

### Good Performance  
- Page Load Time: < 3000ms (3 seconds)
- Time to Interactive: < 3800ms (3.8 seconds)

### Acceptable Performance
- Page Load Time: < 5000ms (5 seconds)
- Time to Interactive: < 5000ms (5 seconds)
- Largest Contentful Paint: < 4000ms
- Cumulative Layout Shift: < 0.25

## Running Power Apps Performance Tests

### Prerequisites
1. Ensure you have access to the Key Vault for client service credentials
2. Make sure Playwright is installed and configured  
3. Verify network access to pre-prod environment

### Simple Commands

#### Main Performance Test
```bash
# Run the main Power Apps performance test
npm run test:performance
```

#### Visual/Debug Testing  
```bash  
# Run with visible browser (great for debugging)
npm run test:performance:headed

# Run in debug mode with breakpoints
npm run test:performance:debug

# Interactive testing with manual control
npm run test:interactive
```

#### View Results
```bash
# Open the HTML report
npx playwright show-report
```

## Test Results and Reports

### What You Get
- **Performance Score** - Overall score (0-100) 
- **Page Load Time** - How long your Power Apps page takes to load
- **Time to Interactive** - When users can actually interact with your app
- **Screenshots** - Visual validation of loaded pages
- **Violation Reports** - Warnings when performance is poor

### Report Locations
- `test-results/html-report/` - HTML test reports
- Test attachments include individual performance reports per test

## Configuration

### Environment Configuration
The test uses a modular URL structure:
- **Base URL**: `https://uc1sdpre-prod.crm4.dynamics.com/main.aspx?appid=5a72304e-f941-f011-877a-6045bda0da75&forceUCI=1&perf=true`
- **Page Parameters**: Each test defines its specific page parameters (e.g., `&pagetype=entityrecord&etn=ktr_product&id=...`)
- **Authentication**: Client Service Account from Azure Key Vault

### Customizing Thresholds
You can adjust performance thresholds in `performance-config.json`:

```json
{
  "performanceThresholds": {
    "good": {
      "pageLoadTime": 3000,      // 3 seconds
      "timeToInteractive": 3800  // 3.8 seconds  
    }
  }
}
```

## Performance Optimization Tips

### Based on Your Results
The tests will show you:
- **Page Load Time** - How long your Power Apps page takes to fully load
- **Time to Interactive** - When users can actually start using your app
- **Performance Score** - Overall score to track improvement over time

## Troubleshooting

### Common Issues
1. **Authentication Failures** - Check Key Vault access and credentials
2. **Network Timeouts** - Your Power Apps page might be loading slowly
3. **Performance Issues** - Use headed mode to see what's happening: `npm run test:performance:headed`

### Debug Mode
Use visual debugging to see exactly what's happening:

```bash
# Run with visible browser
npm run test:performance:headed

# Interactive mode with manual control  
npm run test:interactive
```

## Next Steps

1. **Run your first test**: `npm run test:performance`
2. **Check your scores** - Aim for Page Load < 3s and Time to Interactive < 3.8s
3. **Use headed mode** if you need to debug: `npm run test:performance:headed`
4. **Monitor regularly** - Set up regular testing to catch performance regressions