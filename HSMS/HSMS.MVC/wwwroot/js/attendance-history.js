// Khởi tạo bộ lọc kết quả điểm danh và đặt lại các lựa chọn phụ thuộc khi đổi con hoặc học kỳ.
(() => {
    "use strict";
    const form = document.getElementById("attendance-history-filters");
    if (!form) return;

    const initial = [...form.elements].map(field => ({ field, disabled: field.disabled }));
    form.querySelectorAll("select[data-reset]").forEach(select => {
        // Gửi lại bộ lọc sau khi bỏ các lựa chọn không còn phù hợp với con hoặc học kỳ mới.
        select.addEventListener("change", () => {
            // Đổi con/học kỳ thì không gửi lựa chọn môn hoặc ngày của bộ lọc cũ.
            select.dataset.reset.split(",").forEach(name => {
                const field = form.elements.namedItem(name);
                if (field) field.disabled = true;
            });
            form.requestSubmit();
        });
    });
    // Khôi phục ô lọc khi quay lại trang từ bộ nhớ của trình duyệt.
    window.addEventListener("pageshow", event => {
        if (event.persisted) initial.forEach(({ field, disabled }) => field.disabled = disabled);
    });
})();
