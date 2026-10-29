const test = require("node:test");
const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const vm = require("node:vm");

const script = fs.readFileSync(path.join(__dirname, "../../HSMS/HSMS.MVC/wwwroot/js/attendance.js"), "utf8");

// Chạy JavaScript thật với các ô nhập và sự kiện giả lập, không cần mở ứng dụng/database.
function createForm({ recorded = false, originalStatus = "PRESENT", status = "PRESENT",
    originalComment = "", comment = "", originalLeave = "", leave = "", expired = false, editable = true } = {}) {
    const handlers = {};
    const statusField = { value: status, addEventListener() {}, closest: () => row };
    const leaveField = { value: leave, required: false };
    const commentField = { value: comment };
    const reason = {
        value: "", required: false, validationMessage: "",
        setCustomValidity(message) { this.validationMessage = message; }
    };
    const row = {
        dataset: { status: originalStatus, comment: originalComment, leave: originalLeave },
        querySelector(selector) {
            return { ".am-status": editable ? statusField : null, ".am-leave": leaveField, ".am-comment": commentField }[selector];
        }
    };
    const save = { disabled: false };
    const banner = { visible: false, classList: { remove() { banner.visible = true; } } };
    const form = {
        dataset: { deadline: "2026-10-28T17:00:00Z" },
        addEventListener(event, handler) { handlers[event] = handler; },
        querySelectorAll(selector) {
            if (selector === ".am-status") return editable ? [statusField] : [];
            if (selector === 'tr[data-recorded="true"]') return recorded ? [row] : [];
            if (selector === "select, textarea, input:not([type=hidden])") return [statusField, leaveField, commentField, reason];
            throw new Error("Unexpected selector: " + selector);
        }
    };
    class Clock extends Date {
        static now() { return Date.parse(expired ? "2026-10-28T17:00:00Z" : "2026-10-28T16:59:59Z"); }
    }
    vm.runInNewContext(script, {
        document: { getElementById(id) { return { "attendance-form": form, "attendance-save": save, "attendance-expired": banner, reason }[id]; } },
        window: { setInterval() {} }, Date: Clock
    });
    return { statusField, leaveField, commentField, reason, save, banner, dispatch: event => handlers[event]?.({ preventDefault() {} }) };
}

test("First-time attendance, including automatic excused absence, does not require an edit reason", () => {
    const form = createForm({ status: "EXCUSED", leave: "50" });
    assert.equal(form.reason.required, false);
    assert.equal(form.leaveField.required, true);
});

test("A changed saved status requires a reason; reverting the change removes that requirement", () => {
    const form = createForm({ recorded: true });
    assert.equal(form.reason.required, false);
    form.statusField.value = "LATE";
    form.dispatch("change");
    assert.equal(form.reason.required, true);
    assert.match(form.reason.validationMessage, /lý do/);
    form.reason.value = "   ";
    form.dispatch("input");
    assert.notEqual(form.reason.validationMessage, "");
    form.reason.value = "Xác nhận lại thông tin";
    form.dispatch("input");
    assert.equal(form.reason.validationMessage, "");
    form.statusField.value = "PRESENT";
    form.dispatch("change");
    assert.equal(form.reason.required, false);
});

test("Changing a comment or approved request also requires an edit reason", () => {
    const form = createForm({ recorded: true, originalStatus: "EXCUSED", status: "EXCUSED", originalLeave: "50", leave: "50" });
    assert.equal(form.reason.required, false);
    form.leaveField.value = "51";
    form.dispatch("change");
    assert.equal(form.reason.required, true);
    form.leaveField.value = "50";
    form.commentField.value = " Bổ sung ghi chú ";
    form.dispatch("input");
    assert.equal(form.reason.required, true);
    form.commentField.value = "   ";
    form.dispatch("input");
    assert.equal(form.reason.required, false);
});

test("Automatic leave suggestion over a saved result requires a reason before correction", () => {
    const form = createForm({ recorded: true, originalStatus: "PRESENT", status: "EXCUSED", leave: "50" });
    assert.equal(form.reason.required, true);
    assert.notEqual(form.reason.validationMessage, "");
});

test("Read-only rows do not demand an edit reason and midnight disables saving", () => {
    assert.equal(createForm({ recorded: true, editable: false }).reason.required, false);
    const form = createForm({ expired: true });
    assert.equal(form.save.disabled, true);
    assert.equal(form.statusField.disabled, true);
    assert.equal(form.banner.visible, true);
});
