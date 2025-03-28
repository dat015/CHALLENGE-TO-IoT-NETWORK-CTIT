// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Xử lý chuyển tab nhanh
document.addEventListener('DOMContentLoaded', function () {
    // Lấy tất cả các tab
    const tabElements = document.querySelectorAll('[data-bs-toggle="tab"]');
    
    // Thêm sự kiện keyboard cho mỗi tab
    tabElements.forEach((tab, index) => {
        tab.addEventListener('keydown', function (e) {
            // Chuyển tab khi nhấn mũi tên trái/phải
            if (e.key === 'ArrowRight' || e.key === 'ArrowLeft') {
                e.preventDefault();
                
                const tabs = Array.from(tabElements);
                let newIndex;
                
                if (e.key === 'ArrowRight') {
                    // Chuyển đến tab tiếp theo, quay lại tab đầu nếu đang ở tab cuối
                    newIndex = index === tabs.length - 1 ? 0 : index + 1;
                } else {
                    // Chuyển đến tab trước đó, chuyển đến tab cuối nếu đang ở tab đầu
                    newIndex = index === 0 ? tabs.length - 1 : index - 1;
                }
                
                // Kích hoạt tab mới
                const newTab = new bootstrap.Tab(tabs[newIndex]);
                newTab.show();
                tabs[newIndex].focus();
            }
        });
    });

    // Thêm hiệu ứng fade cho tab content
    const tabPanes = document.querySelectorAll('.tab-pane');
    tabPanes.forEach(pane => {
        pane.classList.add('fade');
    });

    // Hiển thị tab active
    const activePane = document.querySelector('.tab-pane.active');
    if (activePane) {
        activePane.classList.add('show');
    }
});

// Thêm hiệu ứng loading khi chuyển tab
document.addEventListener('shown.bs.tab', function (e) {
    // Thêm hiệu ứng show cho tab mới
    const targetPane = document.querySelector(e.target.getAttribute('href'));
    if (targetPane) {
        setTimeout(() => {
            targetPane.classList.add('show');
        }, 150);
    }
});
