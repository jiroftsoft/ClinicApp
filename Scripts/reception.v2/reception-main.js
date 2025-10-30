(function(){
  console.log('🏥 V2: Reception Main Module Initialized');
  
  // Keyboard shortcuts
  $(document).on("keydown", function(e){
    if(e.key==="F2"){ 
      e.preventDefault(); 
      $("#NationalCode").focus(); 
      console.log('🏥 V2: F2 - Focus on National Code');
    }
    if(e.ctrlKey && e.key==="Enter"){ 
      e.preventDefault(); 
      $("#BtnFinalizePOS").click(); 
      console.log('🏥 V2: Ctrl+Enter - Finalize Reception');
    }
  });
  
  // Initialize form state
  $(document).ready(function() {
    console.log('🏥 V2: Form ready, initializing...');
    
    // Set default values
    $("#Quantity").val(1);
    $("#PayPOS").addClass('active btn-primary');
    $("#PayCash").addClass('btn-outline-secondary');
    
    // Initialize tooltips if Bootstrap is available
    if(typeof bootstrap !== 'undefined') {
      var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
      var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
      });
    }
    
    console.log('🏥 V2: Initialization complete');
  });
})();
