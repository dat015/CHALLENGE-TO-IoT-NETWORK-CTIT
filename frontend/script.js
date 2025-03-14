document.addEventListener('DOMContentLoaded', () => {
    const contentDiv = document.getElementById('content');
    const hamburger = document.querySelector('.hamburger');
    const navLinks = document.querySelector('.nav-links');
    let carouselSlides, prevBtn, nextBtn, slides;
    let currentSlide = 0;
    let slideInterval;

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
                if (file === 'home.html') {
                    initializeCarousel();
                }
            })
            .catch(error => {
                console.error('Lỗi:', error);
                contentDiv.innerHTML = `<p style="color: red;">Lỗi: ${error.message}</p>`;
            });
    }

    // Hàm khởi tạo carousel
    function initializeCarousel() {
        carouselSlides = document.querySelector('.carousel-slides-home');
        prevBtn = document.querySelector('.prev-btn');
        nextBtn = document.querySelector('.next-btn');
        slides = document.querySelectorAll('.slide1');

        if (!carouselSlides || slides.length === 0) {
            console.error('Không tìm thấy carousel hoặc slide!');
            return;
        }

        function showSlide(index) {
            if (index >= slides.length) currentSlide = 0;
            else if (index < 0) currentSlide = slides.length - 1;
            else currentSlide = index;

            // Tính offset dựa trên chiều rộng của mỗi slide (33.33% của container)
            const offset = -currentSlide * (100 / slides.length);
            carouselSlides.style.transform = `translateX(${offset}%)`;
        }

        function startCarousel() {
            slideInterval = setInterval(() => {
                currentSlide++;
                showSlide(currentSlide);
            }, 4000);
        }

        carouselSlides.addEventListener('mouseenter', () => {
            clearInterval(slideInterval);
        });

        carouselSlides.addEventListener('mouseleave', () => {
            startCarousel();
        });

        prevBtn.addEventListener('click', () => {
            clearInterval(slideInterval);
            currentSlide--;
            showSlide(currentSlide);
            startCarousel();
        });

        nextBtn.addEventListener('click', () => {
            clearInterval(slideInterval);
            currentSlide++;
            showSlide(currentSlide);
            startCarousel();
        });

        showSlide(currentSlide);
        startCarousel();
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

    loadContent('home.html');

    // Hàm khởi tạo Dashboard (giữ nguyên)
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
                        x: {
                            display: true,
                            title: {
                                display: true,
                                text: 'Thời gian',
                                color: '#333333',
                                font: { size: 12 }
                            },
                            ticks: { color: '#333333' }
                        },
                        y: {
                            display: true,
                            title: {
                                display: true,
                                text: 'Giá trị',
                                color: '#333333',
                                font: { size: 12 }
                            },
                            beginAtZero: true,
                            ticks: { color: '#333333' }
                        }
                    },
                    plugins: {
                        legend: {
                            display: true,
                            position: 'top',
                            labels: {
                                color: '#333333',
                                font: { size: 12 }
                            }
                        }
                    },
                    hover: { mode: 'nearest', intersect: true },
                    elements: { point: { radius: 3, hoverRadius: 5 } }
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