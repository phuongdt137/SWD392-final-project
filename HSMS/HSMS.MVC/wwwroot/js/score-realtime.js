/*
 * Realtime cho module Đánh giá & Điểm số.
 * Server (ScoreHub) gửi tín hiệu "ScoresUpdated" khi điểm thay đổi. Tín hiệu không chứa dữ liệu điểm:
 * trang tải lại đúng vùng bảng điểm (data-realtime root) từ server, nên quyền xem vẫn do server kiểm tra.
 * Nếu thư viện SignalR không tải được, trang vẫn hoạt động như trước (tải lại thủ công).
 */
(function () {
    'use strict';

    var HUB_URL = '/hubs/scores';
    var EVENT_NAME = 'ScoresUpdated';
    var RETRY_DELAY_MS = 5000;

    /**
     * options:
     *  - rootId: id của vùng cần làm mới (không gồm modal).
     *  - joins: [{ method, argument }] – các phương thức hub cần gọi để vào nhóm theo dõi.
     *  - isBusy(): trả về true khi người dùng đang nhập dở, lúc đó chỉ hiện thông báo thay vì làm mới.
     */
    function start(options) {
        var root = document.getElementById(options.rootId);
        if (!root || typeof signalR === 'undefined') {
            return;
        }

        var isBusy = options.isBusy || function () { return false; };
        var banner = null;

        var connection = new signalR.HubConnectionBuilder()
            .withUrl(HUB_URL)
            .withAutomaticReconnect()
            .build();

        function joinGroups() {
            return Promise.all(options.joins.map(function (join) {
                return connection.invoke(join.method, join.argument);
            }));
        }

        function showBanner() {
            if (banner) {
                return;
            }
            banner = document.createElement('div');
            banner.className = 'alert alert-info d-flex justify-content-between align-items-center gap-3';
            banner.setAttribute('role', 'status');

            var text = document.createElement('span');
            text.textContent = 'Bảng điểm có cập nhật mới. Bạn đang nhập điểm dở: hãy lưu trước, hoặc nhấn Tải lại (nội dung chưa lưu sẽ mất).';

            var button = document.createElement('button');
            button.type = 'button';
            button.className = 'btn btn-sm btn-primary';
            button.textContent = 'Tải lại';
            button.addEventListener('click', refresh);

            banner.appendChild(text);
            banner.appendChild(button);
            root.parentNode.insertBefore(banner, root);
        }

        function hideBanner() {
            if (banner) {
                banner.remove();
                banner = null;
            }
        }

        function refresh() {
            hideBanner();
            return fetch(window.location.href, { credentials: 'same-origin' })
                .then(function (response) {
                    if (response.redirected) {
                        // Phiên hết hạn hoặc mất quyền: để trình duyệt đi theo luồng chuyển hướng của server.
                        window.location.reload();
                        return;
                    }
                    if (!response.ok) {
                        return;
                    }
                    return response.text().then(function (html) {
                        var fresh = new DOMParser().parseFromString(html, 'text/html')
                            .getElementById(options.rootId);
                        if (fresh) {
                            root.innerHTML = fresh.innerHTML;
                        }
                    });
                })
                .catch(function (err) {
                    console.warn('Không tải lại được bảng điểm:', err);
                });
        }

        function onUpdate() {
            if (isBusy()) {
                showBanner();
            } else {
                refresh();
            }
        }

        connection.on(EVENT_NAME, onUpdate);

        // Sau khi kết nối lại, nhóm theo dõi phải vào lại; làm mới một lần để bắt các thay đổi đã bỏ lỡ.
        connection.onreconnected(function () {
            joinGroups().then(onUpdate, function (err) {
                console.warn('Không vào lại được nhóm theo dõi điểm:', err);
            });
        });

        function connect() {
            return connection.start().catch(function () {
                return new Promise(function (resolve) { setTimeout(resolve, RETRY_DELAY_MS); })
                    .then(connect);
            });
        }

        connect()
            .then(joinGroups)
            .catch(function (err) {
                console.warn('Realtime điểm số không khả dụng:', err);
            });
    }

    window.ScoreRealtime = { start: start };
})();
