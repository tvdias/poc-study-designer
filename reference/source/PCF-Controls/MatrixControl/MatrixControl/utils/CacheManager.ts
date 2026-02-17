import { CacheEntry } from '../types/DataServiceTypes';

export class CacheManager {
  private cache = new Map<string, CacheEntry>();
  private pendingRequests = new Map<string, Promise<any>>();
  private cleanupInterval?: number;
  private readonly ttl: number;
  private readonly maxSize: number;
  private readonly cleanupIntervalMs: number;

  constructor(
    ttl: number = 30000, // 30 seconds
    maxSize: number = 1000,
    cleanupIntervalMs: number = 60000 // 1 minute
  ) {
    this.ttl = ttl;
    this.maxSize = maxSize;
    this.cleanupIntervalMs = cleanupIntervalMs;
    this.startCleanup();
  }

  /**
   * Get cached result with LRU tracking
   */
  get<T>(key: string): T | null {
    const entry = this.cache.get(key);
    
    if (!entry) {
      return null;
    }

    // Check if expired
    if (Date.now() - entry.timestamp > this.ttl) {
      this.cache.delete(key);
      return null;
    }

    // Update access tracking for LRU
    entry.accessCount++;
    entry.lastAccessed = Date.now();
    
    return entry.data;
  }

  /**
   * Set cache entry with size management
   */
  set<T>(key: string, data: T): void {
    // Enforce maximum cache size
    if (this.cache.size >= this.maxSize && !this.cache.has(key)) {
      this.evictLeastRecentlyUsed();
    }

    const entry: CacheEntry = {
      data,
      timestamp: Date.now(),
      accessCount: 1,
      lastAccessed: Date.now()
    };

    this.cache.set(key, entry);
  }

  /**
   * Get or fetch with request deduplication
   */
  async getOrFetch<T>(key: string, fetcher: () => Promise<T>): Promise<T> {
    // Check cache first
    const cached = this.get<T>(key);
    if (cached !== null) {
      return cached;
    }

    // Check if request is already pending
    if (this.pendingRequests.has(key)) {
      return this.pendingRequests.get(key);
    }

    // Start new request
    const promise = fetcher();
    this.pendingRequests.set(key, promise);

    try {
      const result = await promise;
      this.set(key, result);
      return result;
    } finally {
      this.pendingRequests.delete(key);
    }
  }

  /**
   * Delete specific cache entry
   */
  delete(key: string): boolean {
    this.pendingRequests.delete(key);
    return this.cache.delete(key);
  }

  /**
   * Delete entries matching pattern
   */
  deleteMatching(pattern: string | RegExp): number {
    let deletedCount = 0;
    const regex = typeof pattern === 'string' ? new RegExp(pattern.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')) : pattern;

    for (const key of this.cache.keys()) {
      if (regex.test(key)) {
        this.cache.delete(key);
        this.pendingRequests.delete(key);
        deletedCount++;
      }
    }

    return deletedCount;
  }

  /**
   * Clear all cache entries
   */
  clear(): void {
    this.cache.clear();
    this.pendingRequests.clear();
  }

  /**
   * Get cache statistics
   */
  getStats(): {
    size: number;
    maxSize: number;
    hitRate: number;
    pendingRequests: number;
    oldestEntry: number | null;
    newestEntry: number | null;
  } {
    const now = Date.now();
    let totalAccess = 0;
    let oldestTimestamp: number | null = null;
    let newestTimestamp: number | null = null;

    for (const entry of this.cache.values()) {
      totalAccess += entry.accessCount;
      
      if (oldestTimestamp === null || entry.timestamp < oldestTimestamp) {
        oldestTimestamp = entry.timestamp;
      }
      
      if (newestTimestamp === null || entry.timestamp > newestTimestamp) {
        newestTimestamp = entry.timestamp;
      }
    }

    const hitRate = this.cache.size > 0 ? (totalAccess / this.cache.size) : 0;

    return {
      size: this.cache.size,
      maxSize: this.maxSize,
      hitRate,
      pendingRequests: this.pendingRequests.size,
      oldestEntry: oldestTimestamp,
      newestEntry: newestTimestamp
    };
  }

  /**
   * Evict least recently used entries
   */
  private evictLeastRecentlyUsed(): void {
    if (this.cache.size === 0) return;

    // Find LRU entry
    let lruKey: string | null = null;
    let lruAccessTime = Infinity;

    for (const [key, entry] of this.cache.entries()) {
      if (entry.lastAccessed < lruAccessTime) {
        lruAccessTime = entry.lastAccessed;
        lruKey = key;
      }
    }

    if (lruKey) {
      this.cache.delete(lruKey);
      this.pendingRequests.delete(lruKey);
    }
  }

  /**
   * Start automatic cleanup process
   */
  private startCleanup(): void {
    this.cleanupInterval = window.setInterval(() => {
      this.cleanup();
    }, this.cleanupIntervalMs);
  }

  /**
   * Clean up expired entries
   */
  private cleanup(): void {
    const now = Date.now();
    const expiredKeys: string[] = [];

    for (const [key, entry] of this.cache.entries()) {
      if (now - entry.timestamp > this.ttl) {
        expiredKeys.push(key);
      }
    }

    for (const key of expiredKeys) {
      this.cache.delete(key);
      this.pendingRequests.delete(key);
    }

    if (expiredKeys.length > 0) {
      console.log(`🧹 Cache cleanup: removed ${expiredKeys.length} expired entries`);
    }
  }

  /**
   * Stop cleanup process and clear cache
   */
  destroy(): void {
    if (this.cleanupInterval) {
      clearInterval(this.cleanupInterval);
      this.cleanupInterval = undefined;
    }
    this.clear();
  }

  /**
   * Invalidate cache entries related to specific entities
   */
  invalidateEntityCache(entityName: string): number {
    const patterns = [
      `${entityName}:`,
      `:${entityName}`,
      `all_junctions:${entityName}`,
      `privileges:${entityName}`
    ];

    let totalDeleted = 0;
    for (const pattern of patterns) {
      totalDeleted += this.deleteMatching(pattern);
    }

    return totalDeleted;
  }
}