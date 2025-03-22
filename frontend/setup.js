function checkWifiSelection() {
    const wifiSelect = document.getElementById("wifiSelect");
    const passwordInput = document.getElementById("passwordInput");
    if (wifiSelect.value !== "") {
        passwordInput.style.display = "block";
    } else {
        passwordInput.style.display = "none";
    }
}

function connectWifi() {
    const wifiSelect = document.getElementById("wifiSelect");
    const wifiPassword = document.getElementById("wifiPassword");
    const wifiStatus = document.getElementById("wifiStatus");

    if (wifiSelect.value === "") {
        wifiStatus.textContent = "Vui lòng chọn mạng Wi-Fi!";
        wifiStatus.style.color = "#ff4444";
        return;
    }

    if (wifiPassword.value === "") {
        wifiStatus.textContent = "Vui lòng nhập mật khẩu!";
        wifiStatus.style.color = "#ff4444";
        return;
    }

    wifiStatus.textContent = "Đang kết nối...";
    wifiStatus.style.color = "#4CAF50";

    fetch('http://localhost:5000/api/wifi/connect', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            ssid: wifiSelect.value,
            password: wifiPassword.value
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.error) {
            wifiStatus.textContent = data.error;
            wifiStatus.style.color = "#ff4444";
        } else {
            wifiStatus.textContent = data.message || `Đã kết nối với ${wifiSelect.value}`;
            wifiStatus.style.color = "#388E3C";
        }
    })
    .catch(error => {
        wifiStatus.textContent = "Lỗi kết nối: " + error.message;
        wifiStatus.style.color = "#ff4444";
    });
}

window.onload = function() {
    const wifiSelect = document.getElementById("wifiSelect");
    const wifiStatus = document.getElementById("wifiStatus");

    if (!wifiStatus) {
        console.error("Không tìm thấy phần tử wifiStatus!");
        return;
    }

    fetch('http://localhost:5069/api/wifi/wifi-list')
        .then(response => {
            if (!response.ok) {
                throw new Error("Không thể lấy danh sách Wi-Fi: " + response.status);
            }
            return response.json();
        })
        .then(data => {
            if (data.message) {
                wifiStatus.textContent = data.message;
                wifiStatus.style.color = "#ff4444";
                return;
            }
            data.forEach(wifi => {
                const option = document.createElement("option");
                option.value = wifi.ssid;
                option.textContent = `${wifi.ssid} (${wifi.signalQuality}%)`;
                wifiSelect.appendChild(option);
            });
            wifiStatus.textContent = "Đã tải danh sách Wi-Fi";
            wifiStatus.style.color = "#388E3C";
        })
        .catch(error => {
            wifiStatus.textContent = "Lỗi: " + error.message;
            wifiStatus.style.color = "#ff4444";
        });
};