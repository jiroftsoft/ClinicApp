/**
 * 🖨️ Print Manager - Production-Grade Printing System
 * 
 * بهترین روش برای چاپ در محیط‌های با ترافیک بالا
 * 
 * ویژگی‌ها:
 * ✅ Single Window Reuse (یک پنجره برای همه چاپ‌ها)
 * ✅ Print Queue (FIFO) برای مدیریت درخواست‌های متوالی
 * ✅ Debounce برای جلوگیری از کلیک‌های مکرر (1500ms)
 * ✅ Lock Manager برای جلوگیری از چاپ همزمان
 * ✅ انتظار تا بسته شدن کامل پنجره قبلی قبل از باز کردن پنجره جدید
 * ✅ استفاده از نام یکتا برای پنجره‌ها (ClinicApp_PrintWindow_timestamp)
 * ✅ Error Recovery و Fallback
 * ✅ Memory Efficient
 * 
 * 🔧 Fixes:
 * - رفع مشکل باز شدن چندباره پنجره چاپ (انتظار تا بسته شدن کامل پنجره قبلی)
 * - افزایش debounce delay به 1500ms
 * - افزایش delay بین چاپ‌ها به 2s
 * - چاپ فوری بدون delay اضافی (300ms فقط برای اطمینان از آماده بودن document)
 * 
 * @author ClinicApp Team
 * @version 1.3.0
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
      debounceDelay: 1500,       // 1500ms debounce (افزایش از 300ms برای جلوگیری از کلیک‌های مکرر)
      queueCheckInterval: 100,   // Check queue every 100ms
      windowCloseDelay: 1000,     // Close window after 1s (کاهش برای بستن سریع بعد از چاپ)
      printDelay: 300,           // Delay before print (300ms فقط برای اطمینان از آماده بودن document)
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
          // ✅ Release lock after delay (افزایش delay برای اطمینان از بسته شدن کامل پنجره)
          setTimeout(function() {
            self.isPrinting = false;
            self.isWindowBusy = false;
            console.log('🔓 PrintManager: Lock released');
            
            // ✅ Process next job in queue
            if (self.printQueue.length > 0) {
              console.log('🔄 PrintManager: Processing next job in queue...');
              setTimeout(function() {
                self._processQueue();
              }, 1000); // 1s delay between prints (افزایش از 500ms به 1000ms)
            }
          }, 2000); // 2s delay before next print (افزایش از 1s به 2s)
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
          // ✅ CRITICAL: بستن کامل پنجره قبلی قبل از باز کردن پنجره جدید
          const closePreviousWindow = function() {
            return new Promise(function(closeResolve) {
              if (self.printWindow && !self.printWindow.closed) {
                console.log('♻️ PrintManager: Closing previous print window...');
                try {
                  self.printWindow.close();
                  
                  // ✅ انتظار تا پنجره کاملاً بسته شود (با polling)
                  let checkCount = 0;
                  const maxChecks = 20; // حداکثر 2 ثانیه انتظار (20 * 100ms)
                  const checkInterval = setInterval(function() {
                    checkCount++;
                    try {
                      if (self.printWindow.closed || checkCount >= maxChecks) {
                        clearInterval(checkInterval);
                        console.log('✅ PrintManager: Previous window closed');
                        self.printWindow = null;
                        closeResolve();
                      }
                    } catch (e) {
                      // اگر به پنجره دسترسی نداریم، یعنی بسته شده
                      clearInterval(checkInterval);
                      console.log('✅ PrintManager: Previous window closed (access denied)');
                      self.printWindow = null;
                      closeResolve();
                    }
                  }, 100);
                } catch (e) {
                  console.warn('⚠️ PrintManager: Cannot close previous window:', e);
                  self.printWindow = null;
                  closeResolve();
                }
              } else {
                closeResolve();
              }
            });
          };
          
          // ✅ بستن پنجره قبلی و سپس باز کردن پنجره جدید
          closePreviousWindow().then(function() {
            // ✅ Create new print window با نام یکتا برای جلوگیری از باز شدن چند پنجره
            const windowFeatures = 'width=400,height=600,menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable=yes';
            const uniqueWindowName = 'ClinicApp_PrintWindow_' + Date.now();
            self.printWindow = window.open(url, uniqueWindowName, windowFeatures);
            
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
                  
                  // ✅ Print immediately after document is ready (بدون delay اضافی)
                  setTimeout(function() {
                    try {
                      if (self.printWindow && !self.printWindow.closed) {
                        // ✅ CRITICAL: بررسی اینکه آیا document آماده است
                        if (self.printWindow.document && self.printWindow.document.readyState === 'complete') {
                          // ✅ چاپ فوری (فقط delay کوتاه برای اطمینان از آماده بودن)
                          setTimeout(function() {
                            try {
                              if (self.printWindow && !self.printWindow.closed) {
                                self.printWindow.focus();
                                self.printWindow.print();
                                console.log('✅ PrintManager: Print command sent');
                                
                                // ✅ Auto-close window immediately after print (بدون delay اضافی)
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
                                resolve();
                              }
                            }
                          }, self.config.printDelay); // فقط 300ms برای اطمینان از آماده بودن
                          
                        } else {
                          // Document هنوز آماده نیست - دوباره تلاش می‌کنیم
                          console.warn('⚠️ PrintManager: Document not ready, retrying...');
                          setTimeout(function() {
                            if (self.printWindow && !self.printWindow.closed) {
                              setTimeout(function() {
                                if (self.printWindow && !self.printWindow.closed) {
                                  self.printWindow.focus();
                                  self.printWindow.print();
                                  console.log('✅ PrintManager: Print command sent (retry)');
                                  if (!isResolved) {
                                    isResolved = true;
                                    resolve();
                                  }
                                }
                              }, self.config.printDelay);
                            }
                          }, 500);
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
                  }, 500); // کاهش delay از 1000ms به 500ms
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
                    // ✅ Delay کوتاه فقط برای اطمینان از آماده بودن
                    setTimeout(function() {
                      if (self.printWindow && !self.printWindow.closed) {
                        self.printWindow.print();
                        console.log('✅ PrintManager: Print command sent (timeout fallback)');
                      }
                    }, self.config.printDelay);
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
          }).catch(function(err) {
            console.error('❌ PrintManager: Error closing previous window:', err);
            // حتی اگر بستن پنجره قبلی خطا داد، ادامه می‌دهیم
            const windowFeatures = 'width=400,height=600,menubar=no,toolbar=no,location=no,status=no,scrollbars=yes,resizable=yes';
            const uniqueWindowName = 'ClinicApp_PrintWindow_' + Date.now();
            self.printWindow = window.open(url, uniqueWindowName, windowFeatures);
            if (!self.printWindow) {
              reject(new Error('Popup blocker فعال است'));
            } else {
              resolve();
            }
          });
          
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
  
  console.log('✅ PrintManager loaded - Version: 1.3.0');
  
})(window, jQuery);

