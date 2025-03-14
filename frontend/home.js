function initializeCarousel() {
    const slidesContainer = document.querySelector('.carousel-slides-home');
    const prevBtn = document.querySelector('.prev-btn');
    const nextBtn = document.querySelector('.next-btn');
    const slides = document.querySelectorAll('.slide1');
    const slideCount = slides.length;
    let currentIndex = 0;

    // Kiểm tra xem các phần tử cần thiết có tồn tại không
    if (!slidesContainer || !prevBtn || !nextBtn || slideCount === 0) {
        console.error('Không tìm thấy carousel hoặc slide trong DOM');
        return;
    }

    // Hàm cập nhật vị trí slide
    function updateSlide() {
        const slideWidth = 100; // Mỗi slide chiếm 100% chiều rộng
        const offset = -currentIndex * slideWidth; // Dịch chuyển từng slide một
        slidesContainer.style.transform = `translateX(${offset}%)`;
    }

    // Chuyển sang slide tiếp theo
    function nextSlide() {
        currentIndex = (currentIndex + 1) % slideCount; // Chỉ tăng 1 slide
        updateSlide();
    }

    // Chuyển về slide trước đó
    function prevSlide() {
        currentIndex = (currentIndex - 1 + slideCount) % slideCount; // Chỉ giảm 1 slide
        updateSlide();
    }

    // Sự kiện bấm nút Next
    nextBtn.addEventListener('click', () => {
        nextSlide();
    });

    // Sự kiện bấm nút Prev
    prevBtn.addEventListener('click', () => {
        prevSlide();
    });

    // Tự động chuyển slide mỗi 5 giây
    let autoSlideInterval = setInterval(() => {
        nextSlide();
    }, 5000);

    // Khởi tạo slide ở vị trí đầu tiên
    updateSlide();

    // Tạm dừng tự động khi hover (tùy chọn)
    slidesContainer.addEventListener('mouseenter', () => {
        clearInterval(autoSlideInterval);
    });

    slidesContainer.addEventListener('mouseleave', () => {
        autoSlideInterval = setInterval(() => {
            nextSlide();
        }, 5000);
    });
}
initializeCarousel();