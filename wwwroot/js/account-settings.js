'use strict';

// Update toggle button UI to match current theme
function updateToggleButton() {
    const currentTheme = getCurrentTheme();
    const icon = document.getElementById('themeIcon');
    const text = document.getElementById('themeText');
    
    if (currentTheme === 'dark') {
        icon.className = 'fas fa-moon';
        text.textContent = 'Dark';
    } else {
        icon.className = 'fas fa-sun';
        text.textContent = 'Light';
    }
}

// Toggle theme
function toggleTheme() {
    const currentTheme = getCurrentTheme();
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    localStorage.setItem('theme', newTheme);
    document.documentElement.setAttribute('data-bs-theme', newTheme);
}

// Get current theme
function getCurrentTheme() {
    return localStorage.getItem('theme') || 'light';
}

//  // Show toast notification
//function showToast(message, type = 'success') {
  // Create toast element
//    const toast = document.createElement('div');
//    toast.className = `toast bg-${type} text-white border-0`;
//    toast.setAttribute('role', 'alert');
//    toast.innerHTML = `
//        <div class="d-flex">
//            <div class="toast-body">
//                ${message}
//            </div>
//            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
//        </div>
//    `;

//    // Add to body
//    document.body.appendChild(toast);

//    // Show toast
//    const bsToast = new bootstrap.Toast(toast);
//    bsToast.show();

//    // Remove after 3 seconds
//    setTimeout(() => {
//        toast.remove();
//    }, 3000);
//}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    // Set initial button state
    updateToggleButton();
    
    // Add click event listener to toggle button
    const toggleBtn = document.getElementById('themeToggleBtn');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function() {
            toggleTheme();
            updateToggleButton();
            showToast('Theme changed successfully', 'success');
        });
    }
    
    // Add change listeners for switches with toast notifications
    const switches = document.querySelectorAll('.toggle-switch');
    switches.forEach(function(switchEl) {
        switchEl.addEventListener('change', function() {
            const settingName = this.closest('.setting-item').querySelector('h3').textContent;
            const status = this.checked ? 'enabled' : 'disabled';
            showToast(settingName + ' ' + status, 'info');
        });
    });
    
    // Add click listeners for connect buttons
    const connectButtons = document.querySelectorAll('.connect-btn');
    connectButtons.forEach(function(btn) {
        btn.addEventListener('click', function() {
            const service = this.closest('.setting-item').querySelector('h3').textContent.replace('Connect with ', '');
            showToast('Connecting to ' + service + '...', 'info');
        });
    });
});