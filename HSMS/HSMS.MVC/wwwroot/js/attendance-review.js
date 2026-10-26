(() => {
    "use strict";
    document.querySelectorAll("form[data-attendance-filter-form]").forEach(form => {
        const initial = [...form.querySelectorAll("select")].map(select => ({ select, disabled: select.disabled, value: select.value }));
        form.addEventListener("change", event => {
            if (!(event.target instanceof HTMLSelectElement)) return;
            for (const name of (event.target.dataset.reset || "").split(",").filter(Boolean)) {
                const dependent = form.elements.namedItem(name);
                if (dependent instanceof HTMLSelectElement) dependent.disabled = true;
            }
            if (form.dataset.autoSubmit === "true" || event.target.dataset.autoSubmit === "true") form.requestSubmit();
        });
        window.addEventListener("pageshow", event => {
            if (!event.persisted) return;
            initial.forEach(({ select, disabled, value }) => { select.disabled = disabled; select.value = value; });
        });
    });
})();
