document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('fertilizerForm');
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const formData = {
            cropCode: parseInt(document.getElementById('cropCode').value),
            timestamp: new Date().toISOString(),
            fertilizerName: document.getElementById('fertilizerName').value,
            quantity: parseInt(document.getElementById('quantity').value),
            description: document.getElementById('description').value
        };
        
        // Here you would typically send the data to your backend
        console.log('Fertilizer Application Data:', formData);
        
        // Show success message
        alert('Fertilizer application data submitted successfully!');
        form.reset();
    });
}); 