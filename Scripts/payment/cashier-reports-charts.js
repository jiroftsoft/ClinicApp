/**
 * Cashier Reports Charts JavaScript
 * مدیریت Charts برای گزارشات صندوق
 * 
 * طبق: CRITICAL-FINANCIAL-MODULE-CONTRACT.md, DEVELOPMENT_CONTRACT.md
 * 
 * استفاده از Chart.js برای نمایش نمودارها
 * 
 * ویژگی‌های کلیدی:
 * - ایجاد Pie Charts
 * - ایجاد Bar Charts
 * - ایجاد Line Charts
 * - ایجاد Area Charts
 * - مدیریت Responsive Charts
 * - پشتیبانی از RTL
 */

var CashierReportsCharts = {
    // تنظیمات پیش‌فرض
    config: {
        defaultColors: {
            primary: '#007bff',
            success: '#28a745',
            warning: '#ffc107',
            danger: '#dc3545',
            info: '#17a2b8',
            secondary: '#6c757d',
            light: '#f8f9fa',
            dark: '#343a40'
        },
        chartOptions: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    rtl: true,
                    labels: {
                        font: {
                            family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                            size: 12
                        },
                        padding: 15,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    rtl: true,
                    titleFont: {
                        family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                        size: 14
                    },
                    bodyFont: {
                        family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                        size: 12
                    },
                    padding: 12,
                    backgroundColor: 'rgba(0, 0, 0, 0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 1
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        font: {
                            family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                            size: 11
                        },
                        callback: function(value) {
                            // Format numbers with Persian digits
                            return new Intl.NumberFormat('fa-IR').format(value);
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    ticks: {
                        font: {
                            family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                            size: 11
                        }
                    },
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        }
    },
    
    // Charts storage
    charts: {},
    
    ensureChartLoaded: function() {
        if (typeof Chart === 'undefined') {
            console.warn('Chart.js is not loaded. Load ~/Scripts/chart.min.js before this script.');
            return false;
        }
        return true;
    },
    
    // Create Pie Chart
    createPieChart: function(canvasId, data, options) {
        var self = this;
        if (!self.ensureChartLoaded()) return null;
        
        // Destroy existing chart if exists
        if (self.charts[canvasId]) {
            self.charts[canvasId].destroy();
        }
        
        var canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('❌ Canvas element not found:', canvasId);
            return null;
        }
        
        var ctx = canvas.getContext('2d');
        
        // Default options
        var defaultOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'top',
                    rtl: true,
                    labels: {
                        font: {
                            family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                            size: 12
                        },
                        padding: 15,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    rtl: true,
                    titleFont: {
                        family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                        size: 14
                    },
                    bodyFont: {
                        family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                        size: 12
                    },
                    padding: 12,
                    callbacks: {
                        label: function(context) {
                            var label = context.label || '';
                            var value = context.parsed || 0;
                            var total = context.dataset.data.reduce(function(a, b) { return a + b; }, 0);
                            var percentage = ((value / total) * 100).toFixed(1);
                            return label + ': ' + new Intl.NumberFormat('fa-IR').format(value) + ' (' + percentage + '%)';
                        }
                    }
                }
            }
        };
        
        // Merge options
        var mergedOptions = $.extend(true, {}, defaultOptions, options || {});
        
        // Create chart
        var chart = new Chart(ctx, {
            type: 'pie',
            data: data,
            options: mergedOptions
        });
        
        // Store chart
        self.charts[canvasId] = chart;
        
        console.log('✅ Pie chart created:', canvasId);
        
        return chart;
    },
    
    // Create Bar Chart
    createBarChart: function(canvasId, data, options) {
        var self = this;
        if (!self.ensureChartLoaded()) return null;
        
        // Destroy existing chart if exists
        if (self.charts[canvasId]) {
            self.charts[canvasId].destroy();
        }
        
        var canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('❌ Canvas element not found:', canvasId);
            return null;
        }
        
        var ctx = canvas.getContext('2d');
        
        // Default options
        var defaultOptions = $.extend(true, {}, self.config.chartOptions, {
            plugins: {
                legend: {
                    position: 'top',
                    rtl: true,
                    labels: {
                        font: {
                            family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                            size: 12
                        },
                        padding: 15,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    rtl: true,
                    callbacks: {
                        label: function(context) {
                            var label = context.dataset.label || '';
                            var value = context.parsed.y || 0;
                            return label + ': ' + new Intl.NumberFormat('fa-IR').format(value);
                        }
                    }
                }
            }
        });
        
        // Merge options
        var mergedOptions = $.extend(true, {}, defaultOptions, options || {});
        
        // Create chart
        var chart = new Chart(ctx, {
            type: 'bar',
            data: data,
            options: mergedOptions
        });
        
        // Store chart
        self.charts[canvasId] = chart;
        
        console.log('✅ Bar chart created:', canvasId);
        
        return chart;
    },
    
    // Create Line Chart
    createLineChart: function(canvasId, data, options) {
        var self = this;
        if (!self.ensureChartLoaded()) return null;
        
        // Destroy existing chart if exists
        if (self.charts[canvasId]) {
            self.charts[canvasId].destroy();
        }
        
        var canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('❌ Canvas element not found:', canvasId);
            return null;
        }
        
        var ctx = canvas.getContext('2d');
        
        // Default options
        var defaultOptions = $.extend(true, {}, self.config.chartOptions, {
            plugins: {
                legend: {
                    position: 'top',
                    rtl: true,
                    labels: {
                        font: {
                            family: 'Vazir, Shabnam, Yekan, Tahoma, Arial, sans-serif',
                            size: 12
                        },
                        padding: 15,
                        usePointStyle: true
                    }
                },
                tooltip: {
                    rtl: true,
                    callbacks: {
                        label: function(context) {
                            var label = context.dataset.label || '';
                            var value = context.parsed.y || 0;
                            return label + ': ' + new Intl.NumberFormat('fa-IR').format(value);
                        }
                    }
                }
            },
            elements: {
                point: {
                    radius: 4,
                    hoverRadius: 6
                },
                line: {
                    tension: 0.4
                }
            }
        });
        
        // Merge options
        var mergedOptions = $.extend(true, {}, defaultOptions, options || {});
        
        // Create chart
        var chart = new Chart(ctx, {
            type: 'line',
            data: data,
            options: mergedOptions
        });
        
        // Store chart
        self.charts[canvasId] = chart;
        
        console.log('✅ Line chart created:', canvasId);
        
        return chart;
    },
    
    // Create Area Chart (Line chart with fill)
    createAreaChart: function(canvasId, data, options) {
        var self = this;
        
        // Ensure datasets have fill property
        if (data.datasets && data.datasets.length > 0) {
            data.datasets.forEach(function(dataset) {
                if (dataset.fill === undefined) {
                    dataset.fill = true;
                }
            });
        }
        
        // Use line chart with fill
        return self.createLineChart(canvasId, data, options);
    },
    
    // Update Chart
    updateChart: function(canvasId, data) {
        var self = this;
        
        if (!self.charts[canvasId]) {
            console.error('❌ Chart not found:', canvasId);
            return;
        }
        
        self.charts[canvasId].data = data;
        self.charts[canvasId].update();
        
        console.log('✅ Chart updated:', canvasId);
    },
    
    // Destroy Chart
    destroyChart: function(canvasId) {
        var self = this;
        
        if (!self.charts[canvasId]) {
            return;
        }
        
        self.charts[canvasId].destroy();
        delete self.charts[canvasId];
        
        console.log('✅ Chart destroyed:', canvasId);
    },
    
    // Destroy All Charts
    destroyAllCharts: function() {
        var self = this;
        
        Object.keys(self.charts).forEach(function(canvasId) {
            self.destroyChart(canvasId);
        });
        
        console.log('✅ All charts destroyed');
    },
    
    // Resize All Charts
    resizeAllCharts: function() {
        var self = this;
        
        Object.keys(self.charts).forEach(function(canvasId) {
            if (self.charts[canvasId]) {
                self.charts[canvasId].resize();
            }
        });
        
        console.log('✅ All charts resized');
    }
};

// Handle window resize
$(window).on('resize', function() {
    if (typeof CashierReportsCharts !== 'undefined') {
        // Debounce resize
        clearTimeout(window.chartResizeTimeout);
        window.chartResizeTimeout = setTimeout(function() {
            CashierReportsCharts.resizeAllCharts();
        }, 250);
    }
});

// Cleanup on page unload
$(window).on('beforeunload', function() {
    if (typeof CashierReportsCharts !== 'undefined') {
        CashierReportsCharts.destroyAllCharts();
    }
});

