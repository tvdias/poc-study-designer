export class Utils {
    /**
     * Generates a GUID (UUID v4)
     * @returns {string} A randomly generated GUID
     */
    static generateGUID(): string {
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        const r = (Math.random() * 16) | 0;
        const v = c === 'x' ? r : (r & 0x3) | 0x8;
        return v.toString(16);
      });
    }
    /**
     * Generates a Random text
     * @returns {string} A randomly generated text of 5 characters length
     */
    static generateText(): string {
      return Array.from({ length: 6 }, () =>
        Math.floor(Math.random() * 6).toString(6)
    ).join('');
    }
  }