
// Simple test to verify that the Jest setup is working correctly

describe('Jest Setup Verification', () => {
    test('should have global Xrm object available', () => {
        expect((global as any).Xrm).toBeDefined();
        expect((global as any).Xrm.WebApi).toBeDefined();
        expect((global as any).Xrm.Navigation).toBeDefined();
        expect((global as any).Xrm.Utility).toBeDefined();
    });

    test('should have ComponentFramework available', () => {
        expect((global as any).ComponentFramework).toBeDefined();
        const cf = (global as any).ComponentFramework;
        expect(cf.Context).toBeDefined();
        expect(cf.PropertyTypes).toBeDefined();
        expect(cf.WebApi).toBeDefined();
    });

    test('should have window.Xrm available', () => {
        expect((window as any).Xrm).toBeDefined();
        expect((window as any).Xrm).toBe((global as any).Xrm);
    });

    test('should have mocked fetch', () => {
        expect((global as any).fetch).toBeDefined();
        expect(jest.isMockFunction((global as any).fetch)).toBe(true);
    });

    test('should have localStorage and sessionStorage mocked', () => {
        expect(window.localStorage).toBeDefined();
        expect(window.sessionStorage).toBeDefined();
        expect(jest.isMockFunction(window.localStorage.getItem)).toBe(true);
        expect(jest.isMockFunction(window.sessionStorage.getItem)).toBe(true);
    });

    test('should have browser APIs mocked', () => {
        // Check that DOM APIs are properly mocked for Jest environment
        expect((global as any).IntersectionObserver).toBeDefined();
        expect((global as any).ResizeObserver).toBeDefined();
        expect(jest.isMockFunction((global as any).IntersectionObserver)).toBe(true);
        expect(jest.isMockFunction((global as any).ResizeObserver)).toBe(true);
        
        // URL might be available globally or need to be accessed via (global as any)
        const urlObject = (global as any).URL;
        expect(urlObject).toBeDefined();
        expect(urlObject.createObjectURL).toBeDefined();
        expect(jest.isMockFunction(urlObject.createObjectURL)).toBe(true);
    });

    test('should clear mocks between tests', () => {
        // Make a call to a mocked function
        window.localStorage.getItem('test');
        expect(window.localStorage.getItem).toHaveBeenCalledWith('test');
        
        // This test verifies that mocks are cleared by the beforeEach in setup.ts
        // If the setup is working correctly, the next test should start with clean mocks
    });

    test('should have clean mocks (continuation of previous test)', () => {
        // If setup.ts is working correctly, this mock should be cleared
        expect(window.localStorage.getItem).not.toHaveBeenCalled();
    });

    test('should handle timers correctly', () => {
        const callback = jest.fn();
        
        // Schedule a timer
        setTimeout(callback, 1000);
        
        // Fast-forward time - this should work now with global fake timers
        jest.advanceTimersByTime(1000);
        
        expect(callback).toHaveBeenCalled();
        expect(callback).toHaveBeenCalledTimes(1);
    });
});

// Export empty object to make this a proper module
export {};