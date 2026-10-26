(() => {
    "use strict";
    const form = document.getElementById("attendance-form");
    if (!form) return;
    const fields = document.getElementById("attendance-fields");
    const checkLock = () => {
        if (form.dataset.canEditAfterDeadline !== "true" && Date.now() >= Date.parse(form.dataset.cutoff)) {
            fields.disabled = true;
            document.getElementById("attendance-lock-message").classList.remove("d-none");
        }
    };
    checkLock();
    window.setInterval(checkLock, 1000);
    document.getElementById("mark-present")?.addEventListener("click", () => {
        form.querySelectorAll("tbody tr").forEach(row => {
            const select = row.querySelector(".attendance-status");
            if (select && !select.value && row.dataset.hasLeave !== "true") select.value = "PRESENT";
        });
    });
    form.addEventListener("submit", event => {
        checkLock();
        if (fields.disabled) event.preventDefault();
    });
})();
