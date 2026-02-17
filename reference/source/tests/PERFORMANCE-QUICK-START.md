# Quick Start Guide - Power Apps Performance Testing

## 🚀 Getting Started

You now have a streamlined performance testing setup focused on the essential metrics for Power Apps! Here's how to get started:

### 1. **Run Your First Performance Test**
Test the specific Power Apps URL:

```powershell
# Navigate to the tests directory
cd tests

# Run the main performance test
npm run test:performance
```

### 2. **View Results with Browser (Interactive)**
```powershell
# Run with visible browser for debugging
npm run test:performance:headed

# Or run interactive testing
npm run test:interactive
```

### 3. **Check Reports**
```powershell
# Open the HTML report
npx playwright show-report
```

## 📊 Power Apps Performance Metrics

### Core Metrics (Simplified for Power Apps)
- **🚀 Page Load Time** - How long it takes to fully load your Power Apps page
- **⚡ Time to Interactive** - When the page becomes fully interactive for users
- **📊 Performance Score** - Overall score (0-100) based on the above metrics

### Reports
- 📈 **Performance Reports** - Clean metrics focused on Power Apps performance
- 📸 **Screenshots** - Visual validation of loaded pages
- ⚠️ **Threshold Violations** - Warnings when performance is poor

## 🎯 Your Base Power Apps URL

The test configuration contains the base Power Apps URL with common parameters:

```text
https://uc1sdpre-prod.crm4.dynamics.com/main.aspx?appid=5a72304e-f941-f011-877a-6045bda0da75&forceUCI=1&perf=true
```

Each test defines its specific page parameters. For example, the Product Entity test adds:
```text
&pagetype=entityrecord&etn=ktr_product&id=4a1d6f47-d308-f011-bae3-0022481a7f97
```

**To add more tests**: Just create new tests that append different page parameters to the base URL!

## 🔧 Authentication

- Uses **Client Service Account** from Azure Key Vault
- Automatically handles login flow
- Pre-configured for pre-prod environment

## 📋 Available Test Commands

```powershell
# Main performance test
npm run test:performance

# Run with visible browser (great for debugging)
npm run test:performance:headed

# Debug mode with breakpoints
npm run test:performance:debug

# Using the PowerShell script (alternative)
.\run-performance-tests.ps1                    # Default run
.\run-performance-tests.ps1 -Headed            # With visible browser
.\run-performance-tests.ps1 -Debug             # Debug mode
.\run-performance-tests.ps1 -ShowReport        # Run and show report
```

## 🎯 Performance Thresholds (Power Apps Optimized)

### ✅ Excellent Performance
- Page Load Time: < 2 seconds
- Time to Interactive: < 2.5 seconds

### ✅ Good Performance  
- Page Load Time: < 3 seconds
- Time to Interactive: < 3.8 seconds

### ⚠️ Acceptable Performance
- Page Load Time: < 5 seconds
- Time to Interactive: < 5 seconds

## 🚨 Next Steps

1. **Run your first test**: `npm run test:performance`
2. **Review the results** and performance score
3. **Use headed mode** for visual debugging: `npm run test:performance:headed`
4. **Set up regular monitoring** by scheduling these tests
5. **Customize thresholds** in the config file if needed

## 💡 Pro Tips

- **Headed mode** (`npm run test:performance:headed`) lets you see what's happening
- **Debug mode** (`npm run test:performance:debug`) allows breakpoints and step-through debugging
- Run tests **multiple times** for reliable results
- Monitor **trends over time** rather than single test results
- Test during **different times** to account for server load

## 🆘 Need Help?

The setup is now simplified! Just run `npm run test:performance:headed` to see your Power Apps performance in action.

Happy performance testing! 🎉