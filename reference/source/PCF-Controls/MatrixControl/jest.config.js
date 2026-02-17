/**
 * Jest Configuration for PCF Project
 */

module.exports = {
    // Use jsdom for React testing
    testEnvironment: 'jsdom',

    // Setup file
    setupFilesAfterEnv: ['<rootDir>/__tests__/setup.ts'],

    // Handle TypeScript files 
    transform: {
        '^.+\\.(ts|tsx)$': ['ts-jest', {
            tsconfig: {
                jsx: 'react', // Changed from 'react-jsx' to 'react'
                esModuleInterop: true,
                allowSyntheticDefaultImports: true
            }
        }],
    },

    // File extensions to handle
    moduleFileExtensions: ['ts', 'tsx', 'js', 'jsx', 'json'],

    // Test file patterns
    testMatch: [
        '<rootDir>/__tests__/**/*.test.{ts,tsx}',
    ],

    // Handle CSS and image imports (removed jsx-runtime mappings)
    moduleNameMapper: {
        '\\.(css|less|scss|sass)$': 'identity-obj-proxy',
        '\\.(jpg|jpeg|png|gif|svg)$': '<rootDir>/__tests__/__mocks__/fileMock.js',
    },

    // Coverage settings
    collectCoverageFrom: [
        'MatrixControl/components/**/*.{ts,tsx}',
        'MatrixControl/utils/**/*.{ts,tsx}',
        'MatrixControl/services/**/*.{ts,tsx}',
        '!**/*.d.ts',
        '!**/__tests__/**',
        '!**/node_modules/**',
    ],

    // Coverage configuration
    collectCoverage: true,
    coverageDirectory: '<rootDir>/coverage',
    coverageReporters: ['text', 'lcov', 'cobertura', 'html'],

    // Test results output
    reporters: [
        'default',
        ['jest-junit', {
            outputDirectory: '<rootDir>/coverage',
            outputName: 'junit.xml'
        }]
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

    // Verbose output for better debugging
    verbose: false,

    // Error handling
    errorOnDeprecated: false
};