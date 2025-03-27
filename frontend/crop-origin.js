document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('cropOriginForm');
    
    form.addEventListener('submit', function(e) {
        e.preventDefault();
        
        const formData = {
            cropCode: parseInt(document.getElementById('cropCode').value),
            timestamp: new Date().toISOString(),
            cropName: document.getElementById('cropName').value,
            location: document.getElementById('location').value,
            farmerName: document.getElementById('farmerName').value,
            description: document.getElementById('description').value,
            sensorId: parseInt(document.getElementById('sensorId').value)
        };
        
        // Here you would typically send the data to your backend
        console.log('Crop Origin Data:', formData);
        
        // Show success message
        alert('Crop origin data submitted successfully!');
        form.reset();
    });
}); 