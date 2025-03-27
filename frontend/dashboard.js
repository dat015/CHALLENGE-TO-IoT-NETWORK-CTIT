document.addEventListener('DOMContentLoaded', function() {
    const menuItems = document.querySelectorAll('.menu-item');
    const contentAreas = document.querySelectorAll('.content-area');
    const backButtons = document.querySelectorAll('.back-button');
    const menuGrid = document.querySelector('.menu-grid');

    // Function to show selected content and hide others
    function showContent(pageId) {
        // Hide all content areas
        contentAreas.forEach(area => {
            area.classList.remove('active');
        });
        
        // Show selected content area
        const selectedContent = document.getElementById(`${pageId}-content`);
        if (selectedContent) {
            selectedContent.classList.add('active');
            menuGrid.style.display = 'none';
        }
    }

    // Function to show menu
    function showMenu() {
        menuGrid.style.display = 'grid';
        contentAreas.forEach(area => {
            area.classList.remove('active');
        });
    }

    // Add click event listeners to menu items
    menuItems.forEach(item => {
        item.addEventListener('click', function() {
            const pageId = this.getAttribute('data-page');
            showContent(pageId);
        });
    });

    // Add click event listeners to back buttons
    backButtons.forEach(button => {
        button.addEventListener('click', showMenu);
    });
}); 