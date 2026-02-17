/**
 * Test Setup File
 */

// Import Jest DOM matchers
import '@testing-library/jest-dom';

// Mock global objects that PCF expects
declare global {
    var Xrm: any;
}

// Basic Xrm mock for PCF compatibility
(global as any).Xrm = {
    WebApi: {
        retrieveMultipleRecords: jest.fn(),
        createRecord: jest.fn(),
        deleteRecord: jest.fn(),
        updateRecord: jest.fn(),
        executeBatch: jest.fn()
    },
    Navigation: {
        openForm: jest.fn(),
        openUrl: jest.fn()
    },
    Utility: {
        formatValue: jest.fn(),
        lookupObjects: jest.fn()
    }
};

// Ensure window has Xrm
if (typeof window !== 'undefined') {
    (window as any).Xrm = (global as any).Xrm;
}

// Mock ComponentFramework
if (!(global as any).ComponentFramework) {
    (global as any).ComponentFramework = {
        Context: {},
        PropertyTypes: {
            StringProperty: 'StringProperty',
            NumberProperty: 'NumberProperty',
            BooleanProperty: 'BooleanProperty'
        },
        WebApi: {}
    };
}

// Mock fetch for HTTP requests
(global as any).fetch = jest.fn(() =>
    Promise.resolve({
        ok: true,
        status: 200,
        json: () => Promise.resolve({}),
        text: () => Promise.resolve('')
    })
);

// Mock localStorage and sessionStorage
const storageMock = {
    getItem: jest.fn(),
    setItem: jest.fn(),
    removeItem: jest.fn(),
    clear: jest.fn(),
    length: 0,
    key: jest.fn()
};

Object.defineProperty(window, 'localStorage', { value: storageMock });
Object.defineProperty(window, 'sessionStorage', { value: storageMock });

// Mock window APIs 
(global as any).IntersectionObserver = jest.fn().mockImplementation(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn()
}));

(global as any).ResizeObserver = jest.fn().mockImplementation(() => ({
    observe: jest.fn(),
    unobserve: jest.fn(),
    disconnect: jest.fn()
}));

// Mock URL APIs
if (typeof (global as any).URL !== 'undefined') {
    (global as any).URL.createObjectURL = jest.fn(() => 'mocked-url');
    (global as any).URL.revokeObjectURL = jest.fn();
} else {
    (global as any).URL = {
        createObjectURL: jest.fn(() => 'mocked-url'),
        revokeObjectURL: jest.fn()
    };
}

// Clear all mocks before each test
beforeEach(() => {
    jest.clearAllMocks();
});

// Suppress console warnings during tests for cleaner output
const originalError = console.error;
const originalWarn = console.warn;

beforeAll(() => {
    console.error = (...args: any[]) => {
        if (
            typeof args[0] === 'string' && 
            (args[0].includes('Warning:') || 
             args[0].includes('React') ||
             args[0].includes('fake timers'))
        ) {
            return;
        }
        originalError.call(console, ...args);
    };
    
    console.warn = (...args: any[]) => {
        if (typeof args[0] === 'string' && 
            (args[0].includes('Warning:') || 
             args[0].includes('fake timers'))) {
            return;
        }
        originalWarn.call(console, ...args);
    };
});

afterAll(() => {
    console.error = originalError;
    console.warn = originalWarn;
});

console.log('Test setup loaded successfully!');