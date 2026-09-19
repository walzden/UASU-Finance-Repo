// Warns the user shortly before the sliding-expiration cookie (see
// Program.cs) times out from inactivity, and logs them out automatically
// if they never respond. Keep WARNING_SECONDS comfortably under the
// server's idle window so the warning has time to show.
(function () {
    var IDLE_MINUTES = 30;
    var WARNING_SECONDS = 60;
    var IDLE_MS = IDLE_MINUTES * 60 * 1000 - WARNING_SECONDS * 1000;
    var ACTIVITY_EVENTS = ['mousemove', 'mousedown', 'keydown', 'scroll', 'touchstart'];

    var modalEl = document.getElementById('idleTimeoutModal');
    var countdownEl = document.getElementById('idleTimeoutCountdown');
    var logoutForm = document.getElementById('idle-logout-form');
    var stayBtn = document.getElementById('idleTimeoutStayBtn');
    var logoutBtn = document.getElementById('idleTimeoutLogoutBtn');
    if (!modalEl || !countdownEl || !logoutForm || !stayBtn || !logoutBtn || !window.bootstrap) {
        return;
    }

    var modal = new bootstrap.Modal(modalEl, { backdrop: 'static', keyboard: false });
    var warnTimer, logoutTimer, countdownInterval;

    function doLogout() {
        logoutForm.submit();
    }

    function showWarning() {
        ACTIVITY_EVENTS.forEach(function (evt) { document.removeEventListener(evt, resetIdleTimer); });

        var remaining = WARNING_SECONDS;
        countdownEl.textContent = remaining;
        modal.show();

        countdownInterval = setInterval(function () {
            remaining -= 1;
            countdownEl.textContent = Math.max(remaining, 0);
            if (remaining <= 0) {
                clearInterval(countdownInterval);
            }
        }, 1000);

        logoutTimer = setTimeout(doLogout, WARNING_SECONDS * 1000);
    }

    function resetIdleTimer() {
        clearTimeout(warnTimer);
        warnTimer = setTimeout(showWarning, IDLE_MS);
    }

    function stayLoggedIn() {
        clearTimeout(logoutTimer);
        clearInterval(countdownInterval);
        modal.hide();
        fetch('/Account/KeepAlive', { credentials: 'same-origin' }).catch(function () {});
        ACTIVITY_EVENTS.forEach(function (evt) { document.addEventListener(evt, resetIdleTimer, { passive: true }); });
        resetIdleTimer();
    }

    stayBtn.addEventListener('click', stayLoggedIn);
    logoutBtn.addEventListener('click', doLogout);

    ACTIVITY_EVENTS.forEach(function (evt) { document.addEventListener(evt, resetIdleTimer, { passive: true }); });
    resetIdleTimer();
})();
