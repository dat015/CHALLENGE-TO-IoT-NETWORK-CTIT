// script.js
document.addEventListener('DOMContentLoaded', () => {
    const contentDiv = document.getElementById('content');

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
                    initializeDashboard(); // Khởi tạo dashboard nếu là content.html
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                contentDiv.innerHTML = `<p style="color: red;">Lỗi: ${error.message}</p>`;
            });
    }

    // Sử dụng Event Delegation để xử lý click trên tất cả các liên kết có data-section
    document.body.addEventListener('click', (e) => {
        const link = e.target.closest('a[data-section]');
        if (link) {
            e.preventDefault(); // Ngăn hành vi mặc định của liên kết
            const sectionFile = link.getAttribute('data-section');
            if (sectionFile) {
                loadContent(sectionFile);
            }
        }
    });

    // Tải Trang chủ mặc định khi trang được load
    loadContent('home.html');

    // Hàm khởi tạo Dashboard (giữ nguyên từ trước)
    function initializeDashboard() {
        const tempChart = createChart('tempChart', 'Nhiệt độ (°C)', '#00ffcc');
        const humidityChart = createChart('humidityChart', 'Độ ẩm không khí (%)', '#ffeb3b');
        const lightChart = createChart('lightChart', 'Ánh sáng (lux)', '#ff4500');
        const soilMoistureChart = createChart('soilMoistureChart', 'Độ ẩm đất (%)', '#32cd32');

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
                        x: { display: true, title: { display: true, text: 'Thời gian', color: '#fff', font: { size: 14 } }, ticks: { color: '#ccc' } },
                        y: { display: true, title: { display: true, text: 'Giá trị', color: '#fff', font: { size: 14 } }, beginAtZero: true, ticks: { color: '#ccc' } }
                    },
                    plugins: { legend: { display: true, position: 'top', labels: { color: '#fff', font: { size: 14 } } } },
                    hover: { mode: 'nearest', intersect: true },
                    elements: { point: { radius: 5, hoverRadius: 7 } }
                }
            });
        }

        function simulateData() {
            return {
                temperature: Math.floor(Math.random() * (35 - 15 + 1)) + 15,
                humidity: Math.floor(Math.random() * (90 - 30 + 1)) + 30,
                light: Math.floor(Math.random() * (1000 - 100 + 1)) + 100,
                soilMoisture: Math.floor(Math.random() * (80 - 20 + 1)) + 20
            };
        }

        setInterval(() => {
            const data = simulateData();
            document.getElementById('temperature').textContent = data.temperature;
            document.getElementById('humidity').textContent = data.humidity;
            document.getElementById('light').textContent = data.light;
            document.getElementById('soilMoisture').textContent = data.soilMoisture;

            document.getElementById('tempStatus').textContent = 'Đã cập nhật';
            document.getElementById('humidStatus').textContent = 'Đã cập nhật';
            document.getElementById('lightStatus').textContent = 'Đã cập nhật';
            document.getElementById('soilStatus').textContent = 'Đã cập nhật';

            updateCharts(data);
        }, 3000);

        function updateCharts(data) {
            const time = new Date().toLocaleTimeString();
            tempChart.data.labels.push(time);
            tempChart.data.datasets[0].data.push(data.temperature);
            humidityChart.data.labels.push(time);
            humidityChart.data.datasets[0].data.push(data.humidity);
            lightChart.data.labels.push(time);
            lightChart.data.datasets[0].data.push(data.light);
            soilMoistureChart.data.labels.push(time);
            soilMoistureChart.data.datasets[0].data.push(data.soilMoisture);

            [tempChart, humidityChart, lightChart, soilMoistureChart].forEach(chart => {
                if (chart.data.labels.length > 10) {
                    chart.data.labels.shift();
                    chart.data.datasets[0].data.shift();
                }
                chart.update();
            });
        }
    }
});