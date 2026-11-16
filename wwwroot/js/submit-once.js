// Stops a form marked data-submit-once from being sent twice by a
// double-click or an impatient second click on a slow connection - the
// usual way an identical voucher gets created twice.
//
// Opt-in per form (not every form in the app) because some forms
// legitimately stay on the page after submitting, e.g. a download.
//
// The button is not disabled inside the submit handler itself: a
// disabled button drops its own name/value from the request, which would
// break buttons that carry a value (Approve/Reject). It is greyed out a
// tick later instead, after the browser has already captured the form.
(function () {
    document.addEventListener('submit', function (e) {
        var form = e.target;
        if (!(form instanceof HTMLFormElement) || !form.hasAttribute('data-submit-once')) return;

        // Another listener (e.g. a confirm() the user cancelled) already
        // stopped this submit - leave the form usable.
        if (e.defaultPrevented) return;

        if (form.dataset.submitting === '1') {
            e.preventDefault();
            return;
        }
        form.dataset.submitting = '1';

        var submitter = e.submitter;
        setTimeout(function () {
            form.querySelectorAll('button[type="submit"], input[type="submit"]').forEach(function (b) {
                b.disabled = true;
            });
            if (submitter && submitter.tagName === 'BUTTON') {
                submitter.dataset.originalText = submitter.textContent;
                submitter.textContent = 'Saving…';
            }
        }, 0);
    });

    // Coming Back to a page restored from the browser's back/forward
    // cache would otherwise leave its form stuck in the "Saving…" state.
    window.addEventListener('pageshow', function (e) {
        if (!e.persisted) return;
        document.querySelectorAll('form[data-submit-once]').forEach(function (form) {
            delete form.dataset.submitting;
            form.querySelectorAll('button[type="submit"], input[type="submit"]').forEach(function (b) {
                b.disabled = false;
                if (b.dataset.originalText) b.textContent = b.dataset.originalText;
            });
        });
    });
})();
