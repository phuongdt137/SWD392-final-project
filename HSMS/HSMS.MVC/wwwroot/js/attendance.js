// Khởi tạo tương tác của bảng điểm danh: chọn đơn nghỉ, yêu cầu lý do sửa và khóa khi hết hạn.
(() => {
    "use strict";
    const form = document.getElementById("attendance-form");
    if (!form) return;

    // Trình duyệt chỉ hỗ trợ giao diện; server kiểm tra lại quyền và hạn khi lưu.
    const save = document.getElementById("attendance-save");
    const deadline = Date.parse(form.dataset.deadline);
    // Khóa ô nhập và nút lưu, đồng thời hiện thông báo khi đã đến hạn điểm danh.
    const checkDeadline = () => {
        if (!save || Date.now() < deadline) return;
        save.disabled = true;
        form.querySelectorAll("select, textarea, input:not([type=hidden])").forEach(input => input.disabled = true);
        document.getElementById("attendance-expired").classList.remove("d-none");
    };
    form.querySelectorAll(".am-status").forEach(select => {
        const leave = select.closest("tr").querySelector(".am-leave");
        // Bắt buộc chọn đơn nghỉ nếu trạng thái của học sinh là Vắng có phép.
        const update = () => { leave.required = select.value === "EXCUSED"; };
        select.addEventListener("change", update);
        update();
    });
    const reason = document.getElementById("reason");
    // Bật yêu cầu nhập lý do khi kết quả đã lưu thay đổi và báo lỗi nếu lý do chỉ có khoảng trắng.
    const updateReason = () => {
        if (!reason) return;
        // Chỉ yêu cầu lý do khi kết quả đã lưu thực sự thay đổi.
        reason.required = [...form.querySelectorAll('tr[data-recorded="true"]')].some(row => {
            const status = row.querySelector(".am-status");
            if (!status) return false; // Dòng chỉ được xem.
            const comment = row.querySelector(".am-comment").value.trim();
            const leave = status.value === "EXCUSED" ? row.querySelector(".am-leave").value : "";
            return status.value !== row.dataset.status || comment !== (row.dataset.comment || "") ||
                leave !== (row.dataset.leave || "");
        });
        reason.setCustomValidity(reason.required && !reason.value.trim() ? "Vui lòng nhập lý do chỉnh sửa điểm danh." : "");
    };
    form.addEventListener("input", updateReason);
    form.addEventListener("change", updateReason);
    updateReason();
    checkDeadline();
    if (save) window.setInterval(checkDeadline, 1000);
    // Kiểm tra hạn lần cuối trên giao diện trước khi gửi biểu mẫu lưu điểm danh.
    form.addEventListener("submit", event => {
        checkDeadline();
        if (save?.disabled) event.preventDefault();
    });
})();
