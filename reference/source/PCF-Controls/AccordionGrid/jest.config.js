const { createDefaultPreset } = require("ts-jest");

const tsJestTransformCfg = createDefaultPreset().transform;

/** @type {import("jest").Config} **/
module.exports = {
  testEnvironment: "jsdom",

  // Setup file
  setupFilesAfterEnv: ['<rootDir>/__tests__/setup.ts'],

  transform: {
    ...tsJestTransformCfg,
  },

  // File extensions to handle
  moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],

   // Test file patterns
  testMatch: [
      '<rootDir>/__tests__/**/*.test.{ts,tsx}',
  ],

  // Clear mocks between tests
  clearMocks: true,

  fakeTimers: {
    enableGlobally: true
  },

  // Timeout for tests
  testTimeout: 10000,

  // Ignore patterns
  testPathIgnorePatterns: [
      '<rootDir>/node_modules/',
      '<rootDir>/lib/',
      '<rootDir>/dist/',
      '<rootDir>/out/',
  ],

  // Coverage configuration
  collectCoverage: true,
  coverageDirectory: '<rootDir>/coverage',
  coverageReporters: ['text', 'lcov', 'cobertura', 'html'],
  collectCoverageFrom: [
    '<rootDir>/src/**/*.{ts,tsx}',
    '!<rootDir>/src/**/*.d.ts',
    '!<rootDir>/src/index.ts'
  ],

  // Test results output
  reporters: [
    'default',
    ['jest-junit', {
      outputDirectory: '<rootDir>/coverage',
      outputName: 'junit.xml'
    }]
  ],

  // Verbose output for better debugging
  verbose: false,

  // Error handling
  errorOnDeprecated: false
};