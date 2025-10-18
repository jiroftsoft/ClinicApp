/**
 * RECEPTION EVENT BUS - سیستم رویداد مرکزی
 * ========================================
 * 
 * این سیستم مسئولیت‌های زیر را دارد:
 * - مدیریت رویدادهای بین ماژول‌ها
 * - Decoupling ماژول‌ها از یکدیگر
 * - مدیریت lifecycle رویدادها
 * - Performance monitoring رویدادها
 * 
 * @author ClinicApp Development Team
 * @version 1.0.0
 * @since 2025-01-17
 */

(function(global) {
    'use strict';

    // ========================================
    // EVENT BUS CONFIGURATION - تنظیمات Event Bus
    // ========================================
    var CONFIG = {
        performance: {
            enableMetrics: true,
            maxListeners: 100,
            maxEventHistory: 1000,
            slowEventThreshold: 100 // ms
        },
        debugging: {
            enableLogging: true,
            logLevel: 'info'
        }
    };

    // ========================================
    // EVENT BUS CLASS - کلاس Event Bus
    // ========================================
    function EventBus() {
        this.listeners = {};
        this.eventHistory = [];
        this.metrics = {
            totalEvents: 0,
            eventsByType: {},
            performance: {
                averageExecutionTime: 0,
                slowestEvent: null,
                fastestEvent: null
            }
        };
        this.isInitialized = false;
    }

    // ========================================
    // INITIALIZATION - راه‌اندازی
    // ========================================
    EventBus.prototype.init = function() {
        if (this.isInitialized) {
            console.warn('[EventBus] Already initialized');
            return;
        }
        
        this.isInitialized = true;
        console.log('[EventBus] Initialized successfully');
    };

    // ========================================
    // EVENT LISTENING - گوش دادن به رویدادها
    // ========================================
    EventBus.prototype.on = function(eventName, callback, options) {
        if (!eventName || typeof callback !== 'function') {
            throw new Error('Event name and callback are required');
        }

        options = options || {};
        
        if (Object.keys(this.listeners).length >= CONFIG.performance.maxListeners) {
            console.warn('Maximum listeners reached');
            return;
        }

        var listener = {
            id: this.generateListenerId(),
            callback: callback,
            options: {
                once: options.once || false,
                priority: options.priority || 0,
                context: options.context || null
            }
        };

        if (!this.listeners[eventName]) {
            this.listeners[eventName] = [];
        }

        this.listeners[eventName].push(listener);
        this.sortListenersByPriority(eventName);

        return listener.id;
    };

    // ========================================
    // EVENT EMISSION - ارسال رویداد
    // ========================================
    EventBus.prototype.emit = function(eventName, data, options) {
        data = data || null;
        options = options || {};
        
        if (!eventName) {
            throw new Error('Event name is required');
        }

        var startTime = performance.now();
        
        try {
            var eventListeners = this.listeners[eventName];
            if (!eventListeners || eventListeners.length === 0) {
                return;
            }

            var results = [];
            var listenersToRemove = [];

            for (var i = 0; i < eventListeners.length; i++) {
                var listener = eventListeners[i];
                try {
                    var result = this.executeListener(listener, data, options);
                    results.push(result);

                    if (listener.options.once) {
                        listenersToRemove.push(listener.id);
                    }
                } catch (error) {
                    console.error('Error in event listener:', error);
                }
            }

            // حذف listeners یکباره
            for (var j = 0; j < listenersToRemove.length; j++) {
                this.removeListener(eventName, listenersToRemove[j]);
            }

            var executionTime = performance.now() - startTime;
            this.recordEvent(eventName, data, results);
            this.updateEventMetrics(eventName, executionTime);

            return results;
        } catch (error) {
            console.error('Error emitting event:', error);
            throw error;
        }
    };

    // ========================================
    // LISTENER EXECUTION - اجرای listener
    // ========================================
    EventBus.prototype.executeListener = function(listener, data, options) {
        var context = listener.options.context || this;
        return listener.callback.call(context, data, options);
    };

    // ========================================
    // LISTENER REMOVAL - حذف listener
    // ========================================
    EventBus.prototype.off = function(eventName, listenerId) {
        if (!eventName) {
            this.listeners = {};
            return;
        }

        if (listenerId) {
            this.removeListener(eventName, listenerId);
        } else {
            delete this.listeners[eventName];
        }
    };

    // ========================================
    // REMOVE SPECIFIC LISTENER - حذف listener خاص
    // ========================================
    EventBus.prototype.removeListener = function(eventName, listenerId) {
        var eventListeners = this.listeners[eventName];
        if (!eventListeners) return;

        var index = -1;
        for (var i = 0; i < eventListeners.length; i++) {
            if (eventListeners[i].id === listenerId) {
                index = i;
                break;
            }
        }

        if (index !== -1) {
            eventListeners.splice(index, 1);
            if (eventListeners.length === 0) {
                delete this.listeners[eventName];
            }
        }
    };

    // ========================================
    // ONCE LISTENER - listener یکباره
    // ========================================
    EventBus.prototype.once = function(eventName, callback, options) {
        options = options || {};
        options.once = true;
        return this.on(eventName, callback, options);
    };

    // ========================================
    // EVENT HISTORY - تاریخچه رویدادها
    // ========================================
    EventBus.prototype.recordEvent = function(eventName, data, results) {
        var eventRecord = {
            eventName: eventName,
            data: data,
            results: results,
            timestamp: Date.now(),
            id: 'event_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9)
        };

        this.eventHistory.push(eventRecord);

        if (this.eventHistory.length > CONFIG.performance.maxEventHistory) {
            this.eventHistory.shift();
        }
    };

    // ========================================
    // METRICS MANAGEMENT - مدیریت metrics
    // ========================================
    EventBus.prototype.updateEventMetrics = function(eventName, executionTime) {
        this.metrics.totalEvents++;
        
        if (!this.metrics.eventsByType[eventName]) {
            this.metrics.eventsByType[eventName] = 0;
        }
        this.metrics.eventsByType[eventName]++;

        this.updatePerformanceMetrics({
            eventName: eventName,
            executionTime: executionTime
        });
    };

    // ========================================
    // PERFORMANCE METRICS UPDATE - به‌روزرسانی metrics عملکرد
    // ========================================
    EventBus.prototype.updatePerformanceMetrics = function(metrics) {
        if (!CONFIG.performance.enableMetrics) return;

        var eventName = metrics.eventName;
        var executionTime = metrics.executionTime;
        
        this.metrics.performance.averageExecutionTime = 
            (this.metrics.performance.averageExecutionTime + executionTime) / 2;

        if (!this.metrics.performance.slowestEvent || 
            executionTime > this.metrics.performance.slowestEvent.executionTime) {
            this.metrics.performance.slowestEvent = { eventName: eventName, executionTime: executionTime };
        }

        if (!this.metrics.performance.fastestEvent || 
            executionTime < this.metrics.performance.fastestEvent.executionTime) {
            this.metrics.performance.fastestEvent = { eventName: eventName, executionTime: executionTime };
        }
    };

    // ========================================
    // UTILITY METHODS - متدهای کمکی
    // ========================================
    EventBus.prototype.generateListenerId = function() {
        return 'listener_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    };

    EventBus.prototype.sortListenersByPriority = function(eventName) {
        var eventListeners = this.listeners[eventName];
        if (!eventListeners) return;

        eventListeners.sort(function(a, b) {
            return b.options.priority - a.options.priority;
        });
    };

    // ========================================
    // DEBUGGING METHODS - متدهای debugging
    // ========================================
    EventBus.prototype.getEventHistory = function() {
        return this.eventHistory;
    };

    EventBus.prototype.getMetrics = function() {
        return this.metrics;
    };

    EventBus.prototype.getListeners = function(eventName) {
        return this.listeners[eventName] || [];
    };

    // ========================================
    // CLEANUP - پاکسازی
    // ========================================
    EventBus.prototype.destroy = function() {
        this.listeners = {};
        this.eventHistory = [];
        this.metrics = {
            totalEvents: 0,
            eventsByType: {},
            performance: {
                averageExecutionTime: 0,
                slowestEvent: null,
                fastestEvent: null
            }
        };
    };

    // ========================================
    // EXPORT - صادرات
    // ========================================
    if (typeof module !== 'undefined' && module.exports) {
        module.exports = EventBus;
    } else if (typeof global !== 'undefined') {
        // Create a singleton instance
        global.ReceptionEventBus = new EventBus();
    }

})(typeof window !== 'undefined' ? window : this);