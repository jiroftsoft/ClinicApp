/**
 * SweetAlert2 focus patch - جلوگیری از TypeError وقتی previousActiveElement نود متنی است
 * نودهای متنی (#text) classList ندارند؛ تابع سازنده selector با Array.from(n.classList) خطا می‌دهد.
 * این اسکریپت باید بلافاصله بعد از sweetalert2@11.js لود شود.
 */
(function () {
  'use strict';
  if (typeof window.Sweetalert2 === 'undefined') return;

  function ensureElementNode(node) {
    if (!node) return node;
    if (node.nodeType === 1) return node; // ELEMENT_NODE
    if (node.nodeType === 3 || node.nodeType === 8) { // TEXT_NODE or COMMENT_NODE
      var parent = node.parentElement || node.parentNode;
      if (parent && parent.nodeType === 1) return parent;
    }
    return node;
  }

  var Original = window.Sweetalert2;
  if (!Original || !Original.fire) return;

  var originalFire = Original.fire.bind(Original);
  Original.fire = function () {
    var el = document.activeElement;
    if (el && el.nodeType !== 1 && el.parentElement) {
      try {
        el.parentElement.focus();
      } catch (e) { /* ignore */ }
    }
    return originalFire.apply(this, arguments);
  };

  if (window.Swal && window.Swal === Original) {
    window.Swal.fire = Original.fire;
  }
})();
