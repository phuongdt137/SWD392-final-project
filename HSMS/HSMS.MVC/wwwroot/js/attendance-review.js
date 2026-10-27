// Dùng chung cho bộ lọc lịch quản lý và lịch sử học sinh/phụ huynh.
(() => {
    "use strict";
    document.querySelectorAll("form[data-attendance-filter-form]").forEach(form => {
        // Lưu lựa chọn ban đầu để phục hồi khi trình duyệt đưa trang trở lại từ bộ nhớ điều hướng.
        const initial = [...form.querySelectorAll("select")].map(select => ({ select, disabled: select.disabled, value: select.value }));
        form.addEventListener("change", event => {
            if (!(event.target instanceof HTMLSelectElement)) return;
            // data-reset chỉ ô phụ thuộc: đổi con bỏ kỳ/môn cũ, đổi kỳ bỏ môn cũ.
            // Ô disabled không gửi trong GET, cho phép server chọn giá trị phù hợp mới.
            for (const name of (event.target.dataset.reset || "").split(",").filter(Boolean)) {
                const dependent = form.elements.namedItem(name);
                if (dependent instanceof HTMLSelectElement) dependent.disabled = true;
            }
            // Tự gửi GET theo cấu hình form hoặc ô chọn; request mới không giữ số trang cũ.
            if (form.dataset.autoSubmit === "true" || event.target.dataset.autoSubmit === "true") form.requestSubmit();
        });
        // Khi Back/Forward dùng bfcache, phục hồi các ô đã disabled trước chuyển trang.
        window.addEventListener("pageshow", event => {
            if (!event.persisted) return;
            initial.forEach(({ select, disabled, value }) => { select.disabled = disabled; select.value = value; });
        });
    });
})();
