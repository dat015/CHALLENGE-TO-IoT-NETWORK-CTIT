document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('pesticideForm');
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const formData = {
            cropCode: parseInt(document.getElementById('cropCode').value),
            timestamp: new Date().toISOString(),
            pesticideName: document.getElementById('pesticideName').value,
            quantity: parseInt(document.getElementById('quantity').value),
            description: document.getElementById('description').value
        };
        
        // Here you would typically send the data to your backend
        console.log('Pesticide Application Data:', formData);
        
        // Show success message
        alert('Pesticide application data submitted successfully!');
        form.reset();
    });
}); 