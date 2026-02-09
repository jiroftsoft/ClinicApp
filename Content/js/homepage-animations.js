/**
 * Homepage Animations
 * Handles fade-in animations and IntersectionObserver for sections
 * 
 * Single Responsibility: مدیریت انیمیشن‌های صفحه اصلی
 * طبق: DEVELOPMENT_CONTRACT.md, SRP
 */

(function() {
    'use strict';
    
    document.addEventListener('DOMContentLoaded', function() {
        const mainContent = document.getElementById('mainContent');
        if (mainContent) {
            mainContent.style.opacity = '1';
            mainContent.style.transform = 'translateY(0)';
        }

        // اضافه کردن انیمیشن به بخش‌های مختلف هنگام اسکرول
        const animatedSections = document.querySelectorAll('.animate-section');
        if (animatedSections.length > 0 && 'IntersectionObserver' in window) {
            const observer = new IntersectionObserver(function(entries) {
                entries.forEach(function(entry) {
                    if (entry.isIntersecting) {
                        entry.target.classList.add('animated');
                    }
                });
            }, {
                threshold: 0.1
            });

            animatedSections.forEach(function(section) {
                observer.observe(section);
            });
        }
    });
})();

