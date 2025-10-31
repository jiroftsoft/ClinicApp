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
    
    // ✅ Initialize tooltips if Bootstrap and Popper are available
    try {
      var hasPopper = false;
      try {
        if (typeof window.Popper !== 'undefined' && typeof window.Popper.createPopper === 'function') {
          hasPopper = true;
        } else if (typeof Popper !== 'undefined' && typeof Popper.createPopper === 'function') {
          hasPopper = true;
        }
      } catch (e) {
        hasPopper = false;
      }
      
      if (typeof bootstrap !== 'undefined' && hasPopper && typeof bootstrap.Tooltip !== 'undefined') {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.forEach(function (tooltipTriggerEl) {
          try {
            var existingTooltip = bootstrap.Tooltip.getInstance(tooltipTriggerEl);
            if (existingTooltip) {
              existingTooltip.dispose();
            }
            new bootstrap.Tooltip(tooltipTriggerEl, {
              trigger: 'hover',
              html: true
            });
          } catch (err) {
            // اگر Popper خطا داد، از title استفاده کنیم
            console.warn('🏥 V2: Error creating tooltip in reception-main:', err.message);
            var $el = $(tooltipTriggerEl);
            var title = $el.data('bs-original-title') || $el.attr('title');
            if (title) {
              $el.attr('title', title);
              $el.removeAttr('data-bs-toggle');
            }
          }
        });
      } else if (typeof bootstrap === 'undefined' || !hasPopper) {
        // Popper موجود نیست - از native tooltip استفاده کنیم
        console.warn('🏥 V2: Popper.js not found, using native tooltips');
        $('[data-bs-toggle="tooltip"]').each(function() {
          var $el = $(this);
          var title = $el.data('bs-original-title') || $el.attr('title');
          if (title) {
            $el.attr('title', title);
            $el.removeAttr('data-bs-toggle');
          }
        });
      }
    } catch (err) {
      console.warn('🏥 V2: Error initializing tooltips in reception-main:', err);
      // Fallback: استفاده از title attribute
      $('[data-bs-toggle="tooltip"]').each(function() {
        var $el = $(this);
        var title = $el.data('bs-original-title') || $el.attr('title');
        if (title) {
          $el.attr('title', title);
          $el.removeAttr('data-bs-toggle');
        }
      });
    }
    
    console.log('🏥 V2: Initialization complete');
  });
})();
