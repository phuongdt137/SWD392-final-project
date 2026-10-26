(() => {
    "use strict";
    const bell = document.getElementById("notification-count");
    if (!bell) return;
    const seen = new Set();
    const refresh = async () => {
        try {
            const response = await fetch("/Notifications/Feed", { credentials: "same-origin" });
            if (!response.ok) return;
            const feed = await response.json();
            bell.textContent = String(feed.unread);
            bell.hidden = feed.unread === 0;
            feed.items.forEach(item => seen.add(item.id));
        } catch { /* The persisted feed is available when the connection returns. */ }
    };
    refresh();
    window.addEventListener("focus", refresh);
    // Refresh also covers a temporarily unavailable realtime client or offline delivery.
    window.setInterval(refresh, 30000);
    if (bell.dataset.realtime !== "true" || !window.signalR) return;
    const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/notifications").withAutomaticReconnect().build();
    connection.on("AttendanceNotification", item => {
        if (!seen.has(item.id)) {
            seen.add(item.id);
            const region = document.getElementById("notification-live");
            region.textContent = `${item.title}: ${item.content}`;
            region.classList.remove("d-none");
        }
        refresh();
    });
    connection.onreconnected(refresh);
    connection.onclose(() => window.setTimeout(start, 15000));
    async function start() {
        try { await connection.start(); await refresh(); }
        catch { window.setTimeout(start, 15000); }
    }
    start();
})();
