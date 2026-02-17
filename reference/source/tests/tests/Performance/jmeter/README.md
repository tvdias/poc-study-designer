# 🧪 Designer.jmx – Dataverse Load Test

This JMeter test plan measures the performance of a Dataverse API endpoint used by Power Apps (`kt_projects` list).  
It uses a service principal to authenticate and runs multiple users in parallel.

---

## ⚙️ 1. Install JMeter

### Windows / macOS / Linux
1. Download **Apache JMeter 5.6+** from  
   👉 https://jmeter.apache.org/download_jmeter.cgi
2. Extract it (no install needed).
3. Run:
   - **Windows:** `bin\jmeter.bat`
   - **macOS/Linux:** `bin/jmeter`

### Plugins
1. Open JMeter.
2. Go to **Options ▸ Plugins Manager** → *Available Plugins*. If needed install plugin Manager -> https://jmeter-plugins.org/wiki/PluginsManager/
3. Install:
   - **Custom Thread Groups** (for the Concurrency Thread Group).
   - (Optional) **JSON Plugins** (for JSON Extractor).
4. Restart JMeter.

---

## 🧩 2. Project Structure

```
jmeter/
├── test-plans/
│   └── Designer.jmx             # Main JMeter test plan
├── test-data/                   # CSV files and test data
├── scripts/
│   ├── run-jmeter-test.ps1      # Automated test execution
│   └── run-test-scenarios.ps1   # Quick scenario selection
├── results/                     # Raw test results (.jtl files)
├── reports/                     # Generated HTML reports
└── README.md                    # This file
```

**Test Plan Structure:**
```
Designer.jmx
├── User Defined Variables       # Configs: users, ramp, hold, entityset, etc.
├── Setup Thread Group           # Gets OAuth2 token once
├── Concurrency Thread Group     # Runs the load test
│   ├── GET projects             # Main Dataverse request
│   ├── HTTP Header Manager      # Authorization + headers
│   └── Constant Timer           # Wait time between calls
├── View Results Tree            # (Debug only)
└── Summary Report               # Metrics view
```

---

## 🧠 3. Test Parameters

| Variable | Description | Default |
|-----------|--------------|----------|
| `users` | Concurrent virtual users | 50 |
| `ramp` | Ramp-up time (seconds) | 5 |
| `hold` | Hold duration (seconds) | 0 |
| `top` | Records per request | 30 |

You can override these from the command line using `-J` flags.

---

## ▶️ 4. Run Tests

### 🤖 Automated Execution (Recommended)

Use the provided PowerShell scripts for easier test execution:

**Quick Scenarios:**
```powershell
# Interactive menu with common test scenarios
.\scripts\run-test-scenarios.ps1
```

**Direct Execution:**
```powershell
# Light load test
.\scripts\run-jmeter-test.ps1 -Users 50 -RampUp 5 -OpenReport

# Heavy load test  
.\scripts\run-jmeter-test.ps1 -Users 250 -RampUp 10 -OpenReport

# Custom test
.\scripts\run-jmeter-test.ps1 -Users 100 -RampUp 5 -Hold 30 -OpenReport
```

### 🖥️ Manual Execution (Non-GUI Mode)

Each command generates an HTML report.  
Make sure the output folder (`-o`) does **not** already exist.

### 🪟 Windows (PowerShell or CMD)
```powershell
# 1 user
.\jmeter.bat -n -t test-plans\Designer.jmx -Jusers=1   -Jramp=1  -Jhold=0 -l results\results_001.jtl -e -o reports\report_001
start reports\report_001\index.html

# 50 users
.\jmeter.bat -n -t test-plans\Designer.jmx -Jusers=50  -Jramp=5  -Jhold=0 -l results\results_050.jtl -e -o reports\report_050
start reports\report_050\index.html

# 100 users
.\jmeter.bat -n -t test-plans\Designer.jmx -Jusers=100 -Jramp=5  -Jhold=0 -l results\results_100.jtl -e -o reports\report_100
start reports\report_100\index.html

# 250 users
.\jmeter.bat -n -t test-plans\Designer.jmx -Jusers=250 -Jramp=10 -Jhold=0 -l results\results_250.jtl -e -o reports\report_250
start reports\report_250\index.html
```

---

## 📊 5. Reading Results

After each run, open the generated `index.html` report.

- **Dashboard → Statistics**: Avg, 90th/95th/99th percentiles, throughput.
- **Response Times Over Time**: Stability and ramp pattern.
- **Active Threads**: Confirms expected concurrency.
- **Errors**: 401/403 → authentication; 429 → Dataverse throttling.

---

## 🧩 6. Adding New Tests

To add another Dataverse call:

1. Right-click **GET projects** → **Duplicate** (or Add → Sampler → HTTP Request).  
2. Change:
   - **Path**: e.g. `/api/data/v9.2/<your_entityset>`
   - **Name**: to match the entity.
3. Add new **Transaction Controller** if you want separate metrics per step.
4. Keep the **Header Manager** with `Authorization: Bearer ${__property(access_token)}`.
5. Re-run from CLI as before.

---

## 🧼 7. Cleanup

Before re-running, remove previous reports:
```bash
rm -rf report_*
```
(Windows PowerShell)
```powershell
Remove-Item -Recurse -Force report_*
```

---
