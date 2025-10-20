/**
 * Test Script for Insurance Change Detection
 * This script helps test and debug the insurance form change detection
 */

(function() {
    'use strict';

    // Test function to simulate form changes
    window.testInsuranceChangeDetection = function() {
        console.log('🧪 Testing Insurance Change Detection...');
        
        // Test 1: Check if modules are available
        console.log('📋 Module Availability:');
        console.log('- FormChangeDetector:', typeof window.FormChangeDetector !== 'undefined');
        console.log('- EditModeManager:', typeof window.EditModeManager !== 'undefined');
        console.log('- ValidationEngine:', typeof window.ValidationEngine !== 'undefined');
        console.log('- InsuranceOrchestrator:', typeof window.InsuranceOrchestrator !== 'undefined');
        
        // Test 2: Check form elements
        console.log('📋 Form Elements:');
        var selectors = [
            '#insuranceProvider',
            '#insurancePlan', 
            '#policyNumber',
            '#cardNumber',
            '#supplementaryProvider',
            '#supplementaryPlan',
            '#supplementaryPolicyNumber',
            '#supplementaryExpiry'
        ];
        
        selectors.forEach(function(selector) {
            var $element = $(selector);
            console.log('- ' + selector + ':', $element.length > 0 ? 'Found' : 'Not Found');
        });
        
        // Test 3: Check current form values
        if (window.FormChangeDetector) {
            var currentValues = window.FormChangeDetector.captureFormValues();
            console.log('📋 Current Form Values:', currentValues);
            
            var originalValues = window.FormChangeDetector.originalValues;
            console.log('📋 Original Values:', originalValues);
            
            var changeResult = window.FormChangeDetector.detectChanges();
            console.log('📋 Change Detection Result:', changeResult);
        }
        
        // Test 4: Check edit mode status
        if (window.EditModeManager) {
            var editModeStatus = window.EditModeManager.getEditModeStatus();
            console.log('📋 Edit Mode Status:', editModeStatus);
        }
        
        // Test 5: Check save button
        var $saveBtn = $('#saveInsuranceBtn');
        console.log('📋 Save Button:');
        console.log('- Found:', $saveBtn.length > 0);
        console.log('- Visible:', $saveBtn.is(':visible'));
        console.log('- Disabled:', $saveBtn.prop('disabled'));
        console.log('- Classes:', $saveBtn.attr('class'));
        
        console.log('✅ Insurance Change Detection Test Completed');
    };
    
    // Test function to simulate a form change
    window.simulateInsuranceChange = function() {
        console.log('🧪 Simulating Insurance Form Change...');
        
        // Change primary provider
        var $provider = $('#insuranceProvider');
        if ($provider.length > 0 && $provider.find('option').length > 1) {
            var firstOption = $provider.find('option:eq(1)').val();
            $provider.val(firstOption).trigger('change');
            console.log('✅ Primary provider changed to:', firstOption);
        }
        
        // Wait and check change detection
        setTimeout(function() {
            if (window.FormChangeDetector) {
                var changeResult = window.FormChangeDetector.detectChanges();
                console.log('📋 Change Detection After Simulation:', changeResult);
            }
        }, 500);
    };
    
    // Test function to reset form to original state
    window.resetInsuranceForm = function() {
        console.log('🧪 Resetting Insurance Form...');
        
        if (window.FormChangeDetector) {
            window.FormChangeDetector.updateOriginalValuesFromCurrentForm();
            console.log('✅ Form reset to original state');
        }
        
        if (window.EditModeManager) {
            window.EditModeManager.disableEditMode();
            console.log('✅ Edit mode disabled');
        }
    };
    
    // Auto-run test when DOM is ready
    $(document).ready(function() {
        setTimeout(function() {
            console.log('🧪 Auto-running Insurance Change Detection Test...');
            window.testInsuranceChangeDetection();
        }, 2000);
    });
    
})();
