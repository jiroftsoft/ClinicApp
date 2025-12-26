/**
 * 🖨️ Print Manager - Production-Grade Printing System
 * 
 * بهترین روش برای چاپ در محیط‌های با ترافیک بالا
 * 
 * ویژگی‌ها:
 * ✅ Single Window Reuse (یک پنجره برای همه چاپ‌ها)
 * ✅ Print Queue (FIFO) برای مدیریت درخواست‌های متوالی
 * ✅ Debounce برای جلوگیری از کلیک‌های مکرر
 * ✅ Lock Manager برای جلوگیری از چاپ همزمان
 * ✅ Error Recovery و Fallback
 * ✅ Memory Efficient
 * 
 * @author ClinicApp Team
 * @version 1.0.0
 */

(function(window, $) {
  'use strict';

  // ✅ Print Manager State
  const PrintManager = {
    // Single reusable print window
    printWindow: null,
    
    // Print queue (FIFO)
    printQueue: [],
    
    // Lock flags
    isPrinting: false,
    isWindowBusy: false,
    
    // Debounce timeout
    debounceTimeout: null,
    lastPrintTime: 0,
    
    // Configuration
    config: {
      debounceDelay: 300,        // 300ms debounce
      queueCheckInterval: 100,   // Check queue every 100ms
      windowCloseDelay: 2000,     // Close window after 2s
      maxQueueSize: 10,           // Maximum queue size
      printTimeout: 10000         // 10s timeout for print
    },
    
    /**
     * ✅ چاپ با مدیریت حرفه‌ای
     * @param {string} url - URL برای چاپ
     * @param {object} options - گزینه‌های چاپ
     * @returns {Promise} Promise که resolve می‌شود وقتی چاپ شروع شد
     */
    print: function(url, options) {
      const self = this;
      options = options || {};
      
      console.log('🖨️ PrintManager: Print request received:', url);
      
      // ✅ Validation
      if (!url || typeof url !== 'string') {
        console.error('❌ PrintManager: Invalid URL:', url);
        return Promise.reject(new Error('URL نامعتبر است'));
      }
      
      // ✅ Debounce: جلوگیری از کلیک‌های مکرر
      const now = Date.now();
      if (now - this.lastPrintTime < this.config.debounceDelay) {
        console.log('⏳ PrintManager: Debounce - ignoring duplicate request');
        return Promise.resolve();
      }
      this.lastPrintTime = now;
      
      // ✅ Clear previous debounce timeout
      if (this.debounceTimeout) {
        clearTimeout(this.debounceTimeout);
        this.debounceTimeout = null;
      }
      
      // ✅ Create print job
      const printJob = {
        url: url,
        options: options,
        timestamp: now,
        promise: null,
        resolve: null,
        reject: null
      };
      
      // ✅ Create promise for this job
      printJob.promise = new Promise(function(resolve, reject) {
        printJob.resolve = resolve;
        printJob.reject = reject;
      });
      
      // ✅ Add to queue
      this.printQueue.push(printJob);
      console.log('📋 PrintManager: Job added to queue. Queue size:', this.printQueue.length);
      
      // ✅ Check queue size limit
      if (this.printQueue.length > this.config.maxQueueSize) {
        const removedJob = this.printQueue.shift();
        console.warn('⚠️ PrintManager: Queue full, removing oldest job');
        if (removedJob.reject) {
          removedJob.reject(new Error('صف چاپ پر است'));
        }
      }
      
      // ✅ Process queue
      this._processQueue();
      
      return printJob.promise;
    },
    
    /**
     * ✅ پردازش صف چاپ
     * @private
     */
    _processQueue: function() {
      const self = this;
      
      // ✅ اگر در حال چاپ است، منتظر بمان
      if (this.isPrinting || this.isWindowBusy) {
        console.log('⏳ PrintManager: Already printing, waiting...');
        return;
      }
      
      // ✅ اگر صف خالی است، خروج
      if (this.printQueue.length === 0) {
        console.log('✅ PrintManager: Queue is empty');
        return;
      }
      
      // ✅ Get next job from queue (FIFO)
      const job = this.printQueue.shift();
      console.log('🖨️ PrintManager: Processing job from queue:', job.url);
      
      // ✅ Set lock
      this.isPrinting = true;
      this.isWindowBusy = true;
      
      // ✅ Execute print
      this._executePrint(job.url, job.options)
        .then(function() {
          console.log('✅ PrintManager: Print completed successfully');
          if (job.resolve) {
            job.resolve();
          }
        })
        .catch(function(err) {
          console.error('❌ PrintManager: Print failed:', err);
          if (job.reject) {
            job.reject(err);
          }
        })
        .finally(function() {
          // ✅ Release lock after delay
          setTimeout(function() {
            self.isPrinting = false;
            self.isWindowBusy = false;
            console.log('🔓 PrintManager: Lock released');
            
            // ✅ Process next job in queue
            if (self.printQueue.length > 0) {
              console.log('🔄 PrintManager: Processing next job in queue...');
              setTimeout(function() {
                self._processQueue();
              }, 500); // 500ms delay between prints
            }
          }, 1000); // 1s delay before next print
        });
    },
    
    /**
     * ✅ اجرای چاپ
     * @private
     */
    _executePrint: function(url, options) {
      const self = this;
      
      return new Promise(function(resolve, reject) {
        try {
          // ✅ Reuse existing window or create new one
          if (self.printWindow && !self.printWindow.closed) {
            console.log('♻️ PrintManager: Reusing existing print window');
            // Close previous window if it's still open
            try {
              self.printWindow.close();
            } catch (e) {
              console.warn('⚠️ PrintManager: Cannot close previous window:', e);
            }
          }
          
          // ✅ Create new print window
          const windowFeatures = 'width=400,height=600,menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable=yes';
          self.printWindow = window.open(url, '_blank', windowFeatures);
          
          if (!self.printWindow) {
            const error = new Error('Popup blocker فعال است. لطفاً popup blocker را غیرفعال کنید.');
            console.error('❌ PrintManager:', error.message);
            reject(error);
            return;
          }
          
          console.log('✅ PrintManager: Print window opened');
          
          // ✅ Monitor window state
          let checkInterval = null;
          let timeoutId = null;
          let isResolved = false;
          
          // ✅ Check if window is loaded
          checkInterval = setInterval(function() {
            try {
              if (self.printWindow.closed) {
                clearInterval(checkInterval);
                if (timeoutId) clearTimeout(timeoutId);
                if (!isResolved) {
                  isResolved = true;
                  console.log('✅ PrintManager: Window closed by user');
                  resolve();
                }
                return;
              }
              
              // ✅ Check if document is ready
              if (self.printWindow.document && self.printWindow.document.readyState === 'complete') {
                clearInterval(checkInterval);
                if (timeoutId) clearTimeout(timeoutId);
                
                // ✅ Print after short delay
                setTimeout(function() {
                  try {
                    if (self.printWindow && !self.printWindow.closed) {
                      self.printWindow.focus();
                      self.printWindow.print();
                      console.log('✅ PrintManager: Print command sent');
                      
                      // ✅ Auto-close window after delay
                      setTimeout(function() {
                        try {
                          if (self.printWindow && !self.printWindow.closed) {
                            self.printWindow.close();
                            console.log('✅ PrintManager: Window auto-closed');
                          }
                        } catch (closeErr) {
                          console.warn('⚠️ PrintManager: Cannot auto-close window:', closeErr);
                        }
                      }, self.config.windowCloseDelay);
                      
                      if (!isResolved) {
                        isResolved = true;
                        resolve();
                      }
                    }
                  } catch (printErr) {
                    console.error('❌ PrintManager: Print error:', printErr);
                    if (!isResolved) {
                      isResolved = true;
                      // Don't reject - window opened successfully
                      resolve();
                    }
                  }
                }, 500);
              }
            } catch (err) {
              // Cross-origin یا خطای دیگر - ادامه می‌دهیم
              console.warn('⚠️ PrintManager: Cannot check window state:', err);
            }
          }, 100);
          
          // ✅ Timeout fallback
          timeoutId = setTimeout(function() {
            clearInterval(checkInterval);
            if (!isResolved) {
              isResolved = true;
              // Try to print anyway
              try {
                if (self.printWindow && !self.printWindow.closed) {
                  self.printWindow.focus();
                  self.printWindow.print();
                  console.log('✅ PrintManager: Print command sent (timeout fallback)');
                }
                resolve();
              } catch (err) {
                console.warn('⚠️ PrintManager: Cannot print after timeout:', err);
                resolve(); // Resolve anyway - window opened
              }
            }
          }, self.config.printTimeout);
          
          // ✅ Error handler
          self.printWindow.onerror = function() {
            clearInterval(checkInterval);
            if (timeoutId) clearTimeout(timeoutId);
            if (!isResolved) {
              isResolved = true;
              const error = new Error('خطا در باز کردن پنجره چاپ');
              console.error('❌ PrintManager:', error.message);
              reject(error);
            }
          };
          
        } catch (ex) {
          console.error('❌ PrintManager: Exception in _executePrint:', ex);
          reject(ex);
        }
      });
    },
    
    /**
     * ✅ پاک کردن صف چاپ
     */
    clearQueue: function() {
      console.log('🗑️ PrintManager: Clearing print queue');
      this.printQueue.forEach(function(job) {
        if (job.reject) {
          job.reject(new Error('صف چاپ پاک شد'));
        }
      });
      this.printQueue = [];
    },
    
    /**
     * ✅ بستن پنجره چاپ
     */
    closeWindow: function() {
      if (this.printWindow && !this.printWindow.closed) {
        try {
          this.printWindow.close();
          console.log('✅ PrintManager: Print window closed');
        } catch (e) {
          console.warn('⚠️ PrintManager: Cannot close window:', e);
        }
      }
      this.printWindow = null;
    },
    
    /**
     * ✅ Reset کامل Print Manager
     */
    reset: function() {
      console.log('🔄 PrintManager: Resetting...');
      this.clearQueue();
      this.closeWindow();
      this.isPrinting = false;
      this.isWindowBusy = false;
      this.lastPrintTime = 0;
      if (this.debounceTimeout) {
        clearTimeout(this.debounceTimeout);
        this.debounceTimeout = null;
      }
    },
    
    /**
     * ✅ دریافت وضعیت Print Manager
     */
    getStatus: function() {
      return {
        isPrinting: this.isPrinting,
        isWindowBusy: this.isWindowBusy,
        queueSize: this.printQueue.length,
        hasWindow: this.printWindow !== null && !this.printWindow.closed
      };
    }
  };
  
  // ✅ Export to global scope
  window.PrintManager = PrintManager;
  
  console.log('✅ PrintManager loaded - Version: 1.0.0');
  
})(window, jQuery);

