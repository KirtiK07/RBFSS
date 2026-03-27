// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// File size validation for uploads
function validateFileSize(input, maxSizeMB = 10) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const maxSizeBytes = maxSizeMB * 1024 * 1024;

        if (file.size > maxSizeBytes) {
            alert(`File size must be less than ${maxSizeMB}MB`);
            input.value = '';
            return false;
        }
    }
    return true;
}

// File type validation
function validateFileType(input, allowedExtensions = ['.pdf', '.doc', '.docx', '.txt', '.jpg', '.png', '.gif']) {
    if (input.files && input.files[0]) {
        const file = input.files[0];
        const fileName = file.name.toLowerCase();
        const isValid = allowedExtensions.some(ext => fileName.endsWith(ext.toLowerCase()));

        if (!isValid) {
            alert(`Please select a valid file type: ${allowedExtensions.join(', ')}`);
            input.value = '';
            return false;
        }
    }
    return true;
}

// Initialize file validation on file inputs
document.addEventListener('DOMContentLoaded', function() {
    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach(function(input) {
        input.addEventListener('change', function() {
            validateFileSize(this);
            validateFileType(this);
        });
    });

    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(function(alert) {
        if (!alert.classList.contains('alert-info')) {
            setTimeout(function() {
                alert.style.opacity = '0';
                setTimeout(function() {
                    alert.style.display = 'none';
                }, 300);
            }, 5000);
        }
    });
});

// Confirmation dialogs for delete actions
function confirmDelete(message = 'Are you sure you want to delete this item?') {
    return confirm(message);
}

// Format file sizes
function formatFileSize(bytes) {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
}