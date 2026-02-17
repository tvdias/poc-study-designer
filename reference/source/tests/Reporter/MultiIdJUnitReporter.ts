
// tests/Reporter/MultiIdJUnitReporter.ts
import type {
  Reporter,
  FullConfig,
  Suite,
  TestCase,
  TestResult,
  FullResult
} from '@playwright/test/reporter';
import fs from 'fs';
import path from 'path';

// Accept string | undefined and coerce to a safe XML string.

const xmlEscape = (s: string | undefined): string => {
  const v = s ?? '';
  return v
    .replace(/[\x00-\x1F\x7F-\x9F]/g, '') // remove control chars
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

// Helper: ms → seconds for JUnit "time" attribute
const msToSeconds = (ms: number | undefined) => ((ms ?? 0) / 1000).toFixed(3);

class MultiIdJUnitReporter implements Reporter {
  private testcases: string[] = [];
  private totalFailures = 0;
  private totalTimeMs = 0;

  onTestEnd(test: TestCase, result: TestResult) {
    if (result.retry < test.retries && result.status === 'failed') {
      
    return; // Ignore failed attempts before last retry
  }
    // Only keep TestCaseId annotations with a defined description
    const annotations = test.annotations
      .filter(a => a.type === 'TestCaseId' && !!a.description);

    // Safely read titles
    const classname = test.parent?.title ?? ''; // <- prevent undefined
    const name = test.title ?? '';              // test.title is typically string, but keep defensive

    // Duration handling
    const timeSeconds = msToSeconds(result.duration);
    this.totalTimeMs += result.duration ?? 0;

    // Failure / skipped blocks
    let childBlock = '';
    if (result.status === 'passed') {
      childBlock = ''; // nothing for passed
    } else if (result.status === 'skipped') {
      childBlock = '<skipped/>';
    } else {
      this.totalFailures += 1;
      const message = xmlEscape(result.error?.message ?? result.status);
      const details = xmlEscape(result.error?.stack ?? result.error?.value ?? message);
      childBlock = `
        <failure message="${message}">
          ${details}
        </failure>`.trim();
    }

    // If no TestCaseId annotations are present, emit one generic testcase
    const ids = annotations.length ? annotations.map(a => a.description!) : [''];

    for (const id of ids) {
      this.testcases.push(`
        <testcase
          classname="${xmlEscape(classname)}"
          name="${xmlEscape(name)}"
          id="${xmlEscape(id)}"
          time="${timeSeconds}">
          ${childBlock}
        </testcase>`.trim());
    }
  }

  onEnd(result: FullResult) {
    const suiteTimeSeconds = msToSeconds(this.totalTimeMs);
    const testsCount = this.testcases.length;

    const xml = `<?xml version="1.0" encoding="UTF-8"?>
<testsuite name="Playwright Suite" tests="${testsCount}" failures="${this.totalFailures}" time="${suiteTimeSeconds}">
${this.testcases.join('\n')}
</testsuite>`;

    const outputPath = path.join(process.cwd(), 'results.xml');
    fs.writeFileSync(outputPath, xml, { encoding: 'utf-8' });
    console.log(`JUnit XML written to ${outputPath} (time=${suiteTimeSeconds}s, tests=${testsCount}, failures=${this.totalFailures})`);
  }
}

export default MultiIdJUnitReporter;