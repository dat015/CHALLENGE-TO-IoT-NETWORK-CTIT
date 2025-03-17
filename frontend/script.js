document.addEventListener('DOMContentLoaded', () => {
    const contentDiv = document.getElementById('content');
    const hamburger = document.querySelector('.hamburger');
    const navLinks = document.querySelector('.nav-links');

    // Toggle hamburger menu
    hamburger.addEventListener('click', () => {
        navLinks.classList.toggle('active');
    });

    // Hàm tải nội dung từ file HTML
    function loadContent(file) {
        fetch(file)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`Không thể tải file: ${file} - ${response.statusText}`);
                }
                return response.text();
            })
            .then(data => {
                contentDiv.innerHTML = data;
                if (file === 'content.html') {
                    initializeDashboard();
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                contentDiv.innerHTML = `<p style="color: red;">Lỗi: ${error.message}</p>`;
            });
    }

    // Xử lý click trên tất cả các liên kết có data-section
    document.body.addEventListener('click', (e) => {
        const link = e.target.closest('a[data-section]');
        if (link) {
            e.preventDefault();
            const sectionFile = link.getAttribute('data-section');
            if (sectionFile) {
                loadContent(sectionFile);
                navLinks.classList.remove('active');
            }
        }
    });

    // Mặc định tải content.html
    loadContent('content.html');

    // Hàm khởi tạo Dashboard
    function initializeDashboard() {
        // Khởi tạo 2 biểu đồ (chỉ temperature và humidity)
        const tempChart = createChart('tempChart', 'Nhiệt độ (°C)', '#00ffcc');
        const humidityChart = createChart('humidityChart', 'Độ ẩm không khí (%)', '#ffeb3b');

        // Hàm tạo biểu đồ
        function createChart(canvasId, label, borderColor) {
            const ctx = document.getElementById(canvasId).getContext('2d');
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
                        x: {
                            display: true,
                            title: { display: true, text: 'Thời gian', color: '#333333', font: { size: 12 } },
                            ticks: { color: '#333333' }
                        },
                        y: {
                            display: true,
                            title: { display: true, text: 'Giá trị', color: '#333333', font: { size: 12 } },
                            beginAtZero: true,
                            ticks: { color: '#333333' }
                        }
                    },
                    plugins: {
                        legend: { display: true, position: 'top', labels: { color: '#333333', font: { size: 12 } } }
                    },
                    hover: { mode: 'nearest', intersect: true },
                    elements: { point: { radius: 3, hoverRadius: 5 } }
                }
            });
        }

        // Hàm cập nhật card và biểu đồ
        function updateDashboard(data) {
            if (!data) {
                document.getElementById('tempStatus').textContent = 'Lỗi kết nối';
                document.getElementById('humidStatus').textContent = 'Lỗi kết nối';
                return;
            }

            // Cập nhật card
            document.getElementById('temperature').textContent = `${data.temperature} °C`;
            document.getElementById('humidity').textContent = `${data.humidity} %`;

            document.getElementById('tempStatus').textContent = 'Đã cập nhật';
            document.getElementById('humidStatus').textContent = 'Đã cập nhật';

            // Cập nhật biểu đồ
            const time = new Date().toLocaleTimeString();
            tempChart.data.labels.push(time);
            tempChart.data.datasets[0].data.push(data.temperature);
            humidityChart.data.labels.push(time);
            humidityChart.data.datasets[0].data.push(data.humidity);

            // Giới hạn 10 điểm dữ liệu
            [tempChart, humidityChart].forEach(chart => {
                if (chart.data.labels.length > 10) {
                    chart.data.labels.shift();
                    chart.data.datasets[0].data.shift();
                }
                chart.update();
            });
        }

        // Kết nối với MQTT broker trên HiveMQ Cloud
        const brokerUrl = 'wss://limealkali-qtypzo.a03.euc1.aws.hivemq.cloud:8884/mqtt'; // HiveMQ Cloud WebSocket Secure
        const options = {
            clientId: 'web_client_' + Math.random().toString(16).substr(2, 8),
            username: 'anhnguyeduc04',
            password: 'Anh20102004@',
            clean: true,
            connectTimeout: 4000,
            reconnectPeriod: 1000,
        };

        // Tạo client MQTT
        const client = mqtt.connect(brokerUrl, options);

        // Sự kiện khi kết nối thành công
        client.on('connect', () => {
            console.log('Connected to HiveMQ Cloud broker');
            document.getElementById('tempStatus').textContent = 'Đã kết nối';
            document.getElementById('humidStatus').textContent = 'Đã kết nối';

            // Subscribe vào topic esp32/dht11
            const topic = 'esp32/dht11';
            client.subscribe(topic, (err) => {
                if (err) {
                    console.error('Subscription error:', err);
                    document.getElementById('tempStatus').textContent = 'Lỗi đăng ký topic';
                    document.getElementById('humidStatus').textContent = 'Lỗi đăng ký topic';
                } else {
                    console.log(`Subscribed to ${topic}`);
                }
            });
        });

        // Sự kiện khi nhận được tin nhắn
        client.on('message', (topic, message) => {
            console.log(`Received message from ${topic}: ${message.toString()}`);
            try {
                const data = JSON.parse(message.toString());
                // Dữ liệu có dạng: {"temperature":31.3,"humidity":80.0}
                updateDashboard(data);
            } catch (e) {
                console.error('Invalid JSON:', e);
                document.getElementById('tempStatus').textContent = 'Dữ liệu không hợp lệ';
                document.getElementById('humidStatus').textContent = 'Dữ liệu không hợp lệ';
            }
        });

        // Sự kiện lỗi
        client.on('error', (err) => {
            console.error('Connection error:', err);
            document.getElementById('tempStatus').textContent = 'Lỗi kết nối';
            document.getElementById('humidStatus').textContent = 'Lỗi kết nối';
        });

        // Sự kiện ngắt kết nối
        client.on('close', () => {
            console.log('Connection closed');
            document.getElementById('tempStatus').textContent = 'Ngắt kết nối';
            document.getElementById('humidStatus').textContent = 'Ngắt kết nối';
        });

        // Sự kiện reconnect
        client.on('reconnect', () => {
            console.log('Reconnecting...');
            document.getElementById('tempStatus').textContent = 'Đang kết nối lại...';
            document.getElementById('humidStatus').textContent = 'Đang kết nối lại...';
        });
    }
});
