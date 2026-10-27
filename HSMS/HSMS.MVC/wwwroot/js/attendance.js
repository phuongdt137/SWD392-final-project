// Hỗ trợ biểu mẫu; quyền và thời hạn vẫn được Service kiểm tra độc lập khi POST.
(() => {
    "use strict";
    const form = document.getElementById("attendance-form");
    if (!form) return;
    const fields = document.getElementById("attendance-fields");
    // Qua 00h khóa ô nhập của giáo viên; STAFF/ADMIN có cờ miễn hạn không bị khóa.
    const checkLock = () => {
        if (form.dataset.canEditAfterDeadline !== "true" && Date.now() >= Date.parse(form.dataset.cutoff)) {
            fields.disabled = true;
            document.getElementById("attendance-lock-message").classList.remove("d-none");
        }
    };
    checkLock();
    window.setInterval(checkLock, 1000);
    // Chỉ điền PRESENT cho dòng chưa chọn và không có đơn nghỉ để người ghi đối chiếu thực tế.
    document.getElementById("mark-present")?.addEventListener("click", () => {
        form.querySelectorAll("tbody tr").forEach(row => {
            const select = row.querySelector(".attendance-status");
            if (select && !select.value && row.dataset.hasLeave !== "true") select.value = "PRESENT";
        });
    });
    // Kiểm tra lại hạn ngay khi gửi, kể cả khi tab vừa được kích hoạt lại.
    form.addEventListener("submit", event => {
        checkLock();
        if (fields.disabled) event.preventDefault();
    });
})();
