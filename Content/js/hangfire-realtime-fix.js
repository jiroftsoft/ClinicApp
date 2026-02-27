/**
 * Hangfire Dashboard — رفع خطای RealtimeGraph وقتی statistics["succeeded:count"] یا statistics["failed:count"] undefined است
 * TypeError: Cannot read properties of undefined (reading 'intValue')
 * پچ روی RealtimeGraph.prototype.appendHistory تا با مقدارهای undefined خطا ندهد.
 */
(function () {
    'use strict';
    function patch() {
        if (typeof window.RealtimeGraph === 'undefined') return false;
        var proto = window.RealtimeGraph.prototype;
        if (!proto || typeof proto.appendHistory !== 'function') return false;
        proto.appendHistory = function (statistics) {
            var s = statistics && statistics['succeeded:count'];
            var f = statistics && statistics['failed:count'];
            var newSucceeded = (s != null && typeof s.intValue !== 'undefined') ? parseInt(s.intValue, 10) : 0;
            var newFailed = (f != null && typeof f.intValue !== 'undefined') ? parseInt(f.intValue, 10) : 0;
            if (isNaN(newSucceeded)) newSucceeded = 0;
            if (isNaN(newFailed)) newFailed = 0;
            var now = Date.now();
            if (this._succeeded !== null && this._failed !== null && (now - this._last < this._pollInterval * 2)) {
                var succeeded = Math.max(newSucceeded - this._succeeded, 0);
                var failed = Math.max(newFailed - this._failed, 0);
                if (this._chart && this._chart.data && this._chart.data.datasets && this._chart.data.datasets[0] && this._chart.data.datasets[1]) {
                    this._chart.data.datasets[0].data.push({ x: new Date(), y: failed });
                    this._chart.data.datasets[1].data.push({ x: new Date(), y: succeeded });
                    this._chart.update();
                }
            }
            this._succeeded = newSucceeded;
            this._failed = newFailed;
            this._last = now;
        };
        return true;
    }
    if (patch()) return;
    var attempts = 0;
    var t = setInterval(function () {
        if (patch() || ++attempts > 50) clearInterval(t);
    }, 20);
})();
