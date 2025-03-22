document.addEventListener('DOMContentLoaded', () => {
    const contentDiv = document.getElementById('content');
    const hamburger = document.querySelector('.hamburger');
    const navLinks = document.querySelector('.nav-links');

    // Kiểm tra xem mqtt có được định nghĩa không
    if (typeof mqtt === 'undefined') {
        console.error('Thư viện mqtt.js không được tải. Vui lòng kiểm tra CDN hoặc file cục bộ.');
        contentDiv.innerHTML = `<p style="color: red;">Lỗi: Thư viện MQTT không được tải.</p>`;
        return;
    }

    // Toggle hamburger menu
    if (hamburger && navLinks) {
        hamburger.addEventListener('click', () => {
            navLinks.classList.toggle('active');
        });
    } else {
        console.error('Hamburger hoặc nav-links không được tìm thấy');
    }

    // Hàm tải nội dung từ file HTML
    function loadContent(file) {
        fetch(file)
            .then(response => {
                if (!response.ok) throw new Error(`Không thể tải file: ${file} - ${response.statusText}`);
                return response.text();
            })
            .then(data => {
                contentDiv.innerHTML = data;
                if (file === 'content.html') {
                    requestAnimationFrame(() => initializeDashboard());
                } else if (file === 'control.html') {
                    requestAnimationFrame(() => initializeControl());
                }
            })
            .catch(error => {
                console.error('Lỗi tải nội dung:', error);
                contentDiv.innerHTML = `<p style="color: red;">Lỗi: ${error.message}</p>`;
            });
    }

    // Xử lý click trên các liên kết
    document.body.addEventListener('click', (e) => {
        const link = e.target.closest('a[data-section]');
        if (link) {
            e.preventDefault();
            const sectionFile = link.getAttribute('data-section');
            if (sectionFile) {
                loadContent(sectionFile);
                if (navLinks) navLinks.classList.remove('active');
            }
        }
    });

    loadContent('content.html');

    let mqttClient; // Biến toàn cục để lưu client MQTT

    // Hàm khởi tạo MQTT client
    function initializeMQTT() {
        const mqttServer = 'wss://test.mosquitto.org:8081/mqtt'; // Sử dụng cổng WebSocket
        const options = {
            clientId: 'anhnguyenduc04/iot_' + Math.random().toString(16).substr(2, 8),
            username: '',
            password: '',
            clean: true,
            connectTimeout: 4000,
            reconnectPeriod: 1000,
            protocolId: 'MQTT',
            protocolVersion: 4
        };

        console.log('Đang kết nối tới MQTT broker:', mqttServer);
        mqttClient = mqtt.connect(mqttServer, options);

        mqttClient.on('connect', () => {
            console.log('Đã kết nối thành công tới test.mosquitto.org qua WebSocket');
            // Cập nhật trạng thái kết nối trên giao diện
            if (document.getElementById('tempStatus')) {
                document.getElementById('tempStatus').textContent = 'Đã kết nối';
                document.getElementById('humidStatus').textContent = 'Đã kết nối';
                document.getElementById('soilStatus').textContent = 'Đã kết nối';
            }
            if (document.getElementById('controlStatus')) {
                document.getElementById('controlStatus').textContent = 'Đã kết nối';
            }
            if (document.getElementById('sleepStatus')) {
                document.getElementById('sleepStatus').textContent = 'Đã kết nối';
            }

            // Subscribe vào topic dữ liệu cảm biến
            const topic = 'anhnguyenduc04/iot';
            mqttClient.subscribe(topic, { qos: 1 }, (err) => {
                if (err) {
                    console.error('Lỗi đăng ký topic:', err);
                    if (document.getElementById('tempStatus')) {
                        document.getElementById('tempStatus').textContent = 'Lỗi đăng ký topic';
                        document.getElementById('humidStatus').textContent = 'Lỗi đăng ký topic';
                        document.getElementById('soilStatus').textContent = 'Lỗi đăng ký topic';
                    }
                } else {
                    console.log(`Đã đăng ký thành công topic: ${topic}`);
                }
            });
        });

        mqttClient.on('error', (err) => {
            console.error('Lỗi kết nối MQTT:', err);
            if (document.getElementById('tempStatus')) {
                document.getElementById('tempStatus').textContent = 'Lỗi kết nối: ' + err.message;
                document.getElementById('humidStatus').textContent = 'Lỗi kết nối: ' + err.message;
                document.getElementById('soilStatus').textContent = 'Lỗi kết nối: ' + err.message;
            }
            if (document.getElementById('controlStatus')) {
                document.getElementById('controlStatus').textContent = 'Lỗi kết nối: ' + err.message;
            }
            if (document.getElementById('sleepStatus')) {
                document.getElementById('sleepStatus').textContent = 'Lỗi kết nối: ' + err.message;
            }
        });

        mqttClient.on('close', () => {
            console.log('Kết nối MQTT đã đóng');
            if (document.getElementById('tempStatus')) {
                document.getElementById('tempStatus').textContent = 'Ngắt kết nối';
                document.getElementById('humidStatus').textContent = 'Ngắt kết nối';
                document.getElementById('soilStatus').textContent = 'Ngắt kết nối';
            }
            if (document.getElementById('controlStatus')) {
                document.getElementById('controlStatus').textContent = 'Ngắt kết nối';
            }
            if (document.getElementById('sleepStatus')) {
                document.getElementById('sleepStatus').textContent = 'Ngắt kết nối';
            }
        });

        mqttClient.on('reconnect', () => {
            console.log('Đang thử kết nối lại...');
            if (document.getElementById('tempStatus')) {
                document.getElementById('tempStatus').textContent = 'Đang kết nối lại...';
                document.getElementById('humidStatus').textContent = 'Đang kết nối lại...';
                document.getElementById('soilStatus').textContent = 'Đang kết nối lại...';
            }
            if (document.getElementById('controlStatus')) {
                document.getElementById('controlStatus').textContent = 'Đang kết nối lại...';
            }
            if (document.getElementById('sleepStatus')) {
                document.getElementById('sleepStatus').textContent = 'Đang kết nối lại...';
            }
        });

        return mqttClient;
    }

    // Khởi tạo Dashboard
    function initializeDashboard() {
        const tempChart = createChart('tempChart', 'Nhiệt độ (°C)', '#00ffcc');
        const humidityChart = createChart('humidityChart', 'Độ ẩm không khí (%)', '#ffeb3b');
        const soilMoistureChart = createChart('soilMoistureChart', 'Độ ẩm đất (%)', '#32cd32');

        if (!tempChart || !humidityChart || !soilMoistureChart) {
            console.error('Không thể khởi tạo một hoặc nhiều biểu đồ');
            document.getElementById('tempStatus').textContent = 'Lỗi khởi tạo biểu đồ';
            document.getElementById('humidStatus').textContent = 'Lỗi khởi tạo biểu đồ';
            document.getElementById('soilStatus').textContent = 'Lỗi khởi tạo biểu đồ';
            return;
        }

        function createChart(canvasId, label, borderColor) {
            const canvas = document.getElementById(canvasId);
            if (!canvas) {
                console.error(`Không tìm thấy canvas với ID ${canvasId}`);
                return null;
            }
            const ctx = canvas.getContext('2d');
            return new Chart(ctx, {
                type: 'line',
                data: {
                    labels: [],
                    datasets: [{
                        label: label,
                        data: [],
                        borderColor: borderColor,
                        backgroundColor: borderColor.replace(')', ', 0.4)'),
                        borderWidth: 4,
                        fill: false,
                        tension: 0.1
                    }]
                },
                options: {
                    scales: {
                        x: { display: true, title: { display: true, text: 'Thời gian', color: '#333333', font: { size: 12 } }, ticks: { color: '#333333' } },
                        y: { display: true, title: { display: true, text: 'Giá trị', color: '#333333', font: { size: 12 } }, beginAtZero: true, ticks: { color: '#333333' } }
                    },
                    plugins: { legend: { display: true, position: 'top', labels: { color: '#333333', font: { size: 12 } } } },
                    hover: { mode: 'nearest', intersect: true },
                    elements: { point: { radius: 3, hoverRadius: 5 } }
                }
            });
        }

        function updateDashboard(data) {
            if (!data) {
                document.getElementById('tempStatus').textContent = 'Lỗi kết nối';
                document.getElementById('humidStatus').textContent = 'Lỗi kết nối';
                document.getElementById('soilStatus').textContent = 'Lỗi kết nối';
                return;
            }

            document.getElementById('temperature').textContent = `${data.temperature || 0} `;
            document.getElementById('humidity').textContent = `${data.humidity || 0} `;
            document.getElementById('soilMoisture').textContent = `${data.soilMoisture || 0} `;

            document.getElementById('tempStatus').textContent = 'Đã cập nhật';
            document.getElementById('humidStatus').textContent = 'Đã cập nhật';
            document.getElementById('soilStatus').textContent = 'Đã cập nhật';

            const time = new Date().toLocaleTimeString();
            tempChart.data.labels.push(time);
            tempChart.data.datasets[0].data.push(data.temperature || 0);
            humidityChart.data.labels.push(time);
            humidityChart.data.datasets[0].data.push(data.humidity || 0);
            soilMoistureChart.data.labels.push(time);
            soilMoistureChart.data.datasets[0].data.push(data.soilMoisture || 0);

            [tempChart, humidityChart, soilMoistureChart].forEach(chart => {
                if (chart.data.labels.length > 10) {
                    chart.data.labels.shift();
                    chart.data.datasets[0].data.shift();
                }
                chart.update();
            });
        }

        // Kết nối MQTT và xử lý dữ liệu cảm biến
        mqttClient = initializeMQTT();

        mqttClient.on('message', (topic, message) => {
            console.log(`Nhận tin nhắn từ ${topic}: ${message.toString()}`);
            try {
                const data = JSON.parse(message.toString());
                console.log('Dữ liệu nhận được:', data);
                updateDashboard(data);
            } catch (e) {
                console.error('JSON không hợp lệ:', e);
                document.getElementById('tempStatus').textContent = 'Dữ liệu không hợp lệ';
                document.getElementById('humidStatus').textContent = 'Dữ liệu không hợp lệ';
                document.getElementById('soilStatus').textContent = 'Dữ liệu không hợp lệ';
            }
        });
    }

    // Khởi tạo trang Cài đặt và Điều khiển
    function initializeControl() {
        // Kết nối MQTT
        mqttClient = initializeMQTT();

        // Định nghĩa trạng thái ban đầu cho nước và đèn
        let waterState = false;
        let lightState = false;

        // Hàm bật/tắt nước
        window.toggleWater = function () {
            waterState = !waterState;
            const command = waterState ? 'WATER_ON' : 'WATER_OFF';
            if (mqttClient && mqttClient.connected) {
                mqttClient.publish('anhnguyenduc04/control', command, { qos: 1 }, (err) => {
                    if (err) {
                        console.error('Lỗi khi gửi lệnh nước:', err);
                        document.getElementById('controlStatus').textContent = 'Lỗi: ' + err.message;
                    } else {
                        console.log(`Đã gửi lệnh: ${command}`);
                        document.getElementById('controlStatus').textContent = waterState ? 'Nước: Bật' : 'Nước: Tắt';
                        document.getElementById('waterButton').textContent = waterState ? 'Tắt Nước' : 'Bật Nước';
                    }
                });
            } else {
                document.getElementById('controlStatus').textContent = 'Chưa kết nối tới MQTT broker';
            }
        };

        // Hàm bật/tắt đèn
        window.toggleLight = function () {
            lightState = !lightState;
            const command = lightState ? 'LIGHT_ON' : 'LIGHT_OFF';
            if (mqttClient && mqttClient.connected) {
                mqttClient.publish('anhnguyenduc04/control', command, { qos: 1 }, (err) => {
                    if (err) {
                        console.error('Lỗi khi gửi lệnh đèn:', err);
                        document.getElementById('controlStatus').textContent = 'Lỗi: ' + err.message;
                    } else {
                        console.log(`Đã gửi lệnh: ${command}`);
                        document.getElementById('controlStatus').textContent = lightState ? 'Đèn: Bật' : 'Đèn: Tắt';
                        document.getElementById('lightButton').textContent = lightState ? 'Tắt Đèn' : 'Bật Đèn';
                    }
                });
            } else {
                document.getElementById('controlStatus').textContent = 'Chưa kết nối tới MQTT broker';
            }
        };

        // Hàm gửi thời gian ngủ
        window.sendSleepTime = function () {
            const sleepTimeInput = document.getElementById('sleepTime');
            const sleepTime = parseInt(sleepTimeInput.value);

            if (isNaN(sleepTime) || sleepTime <= 0) {
                document.getElementById('sleepStatus').textContent = 'Vui lòng nhập số giây hợp lệ (lớn hơn 0)';
                return;
            }

            if (mqttClient && mqttClient.connected) {
                const topic = 'anhnguyenduc04/sleep';
                const message = sleepTime.toString();
                mqttClient.publish(topic, message, { qos: 1 }, (err) => {
                    if (err) {
                        console.error('Lỗi khi gửi thời gian ngủ:', err);
                        document.getElementById('sleepStatus').textContent = 'Lỗi: ' + err.message;
                    } else {
                        console.log(`Đã gửi thời gian ngủ ${sleepTime} giây tới topic ${topic}`);
                        document.getElementById('sleepStatus').textContent = `Đã gửi: Ngủ ${sleepTime} giây`;
                    }
                });
            } else {
                document.getElementById('sleepStatus').textContent = 'Chưa kết nối tới MQTT broker';
            }
        };
    }
});