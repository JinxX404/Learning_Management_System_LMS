/**
 * Toast Notification System
 * Provides a simple API for showing toast notifications
 */

(function() {
    'use strict';

    // Create toast container if it doesn't exist
    function createToastContainer() {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '9999';
            document.body.appendChild(container);
        }
        return container;
    }

    /**
     * Show a toast notification
     * @param {string} message - The message to display
     * @param {string} type - Type of toast: 'success', 'error', 'warning', 'info'
     * @param {number} duration - Duration in milliseconds (default: 5000)
     */
    function showToast(message, type = 'info', duration = 5000) {
        const container = createToastContainer();
        
        // Map types to Bootstrap classes
        const typeMap = {
            'success': { bg: 'bg-success', icon: 'check-circle', text: 'Success' },
            'error': { bg: 'bg-danger', icon: 'x-circle', text: 'Error' },
            'warning': { bg: 'bg-warning', icon: 'alert-triangle', text: 'Warning' },
            'info': { bg: 'bg-info', icon: 'info', text: 'Info' }
        };
        
        const config = typeMap[type] || typeMap['info'];
        
        // Create toast element
        const toastId = 'toast-' + Date.now();
        const toastEl = document.createElement('div');
        toastEl.id = toastId;
        toastEl.className = 'toast';
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');
        
        toastEl.innerHTML = `
            <div class="toast-header ${config.bg} text-white">
                <svg class="me-2" width="16" height="16" fill="currentColor" viewBox="0 0 16 16">
                    ${getIconSvg(config.icon)}
                </svg>
                <strong class="me-auto">${config.text}</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        `;
        
        container.appendChild(toastEl);
        
        // Initialize Bootstrap toast
        const bsToast = new bootstrap.Toast(toastEl, {
            autohide: true,
            delay: duration
        });
        
        // Show toast
        bsToast.show();
        
        // Remove from DOM after hidden
        toastEl.addEventListener('hidden.bs.toast', function() {
            toastEl.remove();
        });
        
        return bsToast;
    }

    /**
     * Get SVG path for icon
     */
    function getIconSvg(iconName) {
        const icons = {
            'check-circle': '<path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zm-3.97-3.03a.75.75 0 0 0-1.08.022L7.477 9.417 5.384 7.323a.75.75 0 0 0-1.06 1.06L6.97 11.03a.75.75 0 0 0 1.079-.02l3.992-4.99a.75.75 0 0 0-.01-1.05z"/>',
            'x-circle': '<path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM5.354 4.646a.5.5 0 1 0-.708.708L7.293 8l-2.647 2.646a.5.5 0 0 0 .708.708L8 8.707l2.646 2.647a.5.5 0 0 0 .708-.708L8.707 8l2.647-2.646a.5.5 0 0 0-.708-.708L8 7.293 5.354 4.646z"/>',
            'alert-triangle': '<path d="M8.982 1.566a1.13 1.13 0 0 0-1.96 0L.165 13.233c-.457.778.091 1.767.98 1.767h13.713c.889 0 1.438-.99.98-1.767L8.982 1.566zM8 5c.535 0 .954.462.9.995l-.35 3.507a.552.552 0 0 1-1.1 0L7.1 5.995A.905.905 0 0 1 8 5zm.002 6a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/>',
            'info': '<path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm.93-9.412-1 4.705c-.07.34.029.533.304.533.194 0 .487-.07.686-.246l-.088.416c-.287.346-.92.598-1.465.598-.703 0-1.002-.422-.808-1.319l.738-3.468c.064-.293.006-.399-.287-.47l-.451-.081.082-.381 2.29-.287zM8 5.5a1 1 0 1 1 0-2 1 1 0 0 1 0 2z"/>'
        };
        return icons[iconName] || icons['info'];
    }

    /**
     * Show success toast
     */
    function showSuccess(message, duration) {
        return showToast(message, 'success', duration);
    }

    /**
     * Show error toast
     */
    function showError(message, duration) {
        return showToast(message, 'error', duration);
    }

    /**
     * Show warning toast
     */
    function showWarning(message, duration) {
        return showToast(message, 'warning', duration);
    }

    /**
     * Show info toast
     */
    function showInfo(message, duration) {
        return showToast(message, 'info', duration);
    }

    /**
     * Show confirmation dialog
     */
    function showConfirm(message, onConfirm, onCancel) {
        const modalId = 'confirm-modal-' + Date.now();
        const modalEl = document.createElement('div');
        modalEl.className = 'modal fade';
        modalEl.id = modalId;
        modalEl.setAttribute('tabindex', '-1');
        modalEl.setAttribute('aria-labelledby', modalId + '-label');
        modalEl.setAttribute('aria-hidden', 'true');
        
        modalEl.innerHTML = `
            <div class="modal-dialog modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="${modalId}-label">Confirm Action</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        ${message}
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                        <button type="button" class="btn btn-primary" id="${modalId}-confirm">Confirm</button>
                    </div>
                </div>
            </div>
        `;
        
        document.body.appendChild(modalEl);
        
        const modal = new bootstrap.Modal(modalEl);
        
        // Handle confirm
        document.getElementById(modalId + '-confirm').addEventListener('click', function() {
            if (onConfirm) onConfirm();
            modal.hide();
        });
        
        // Handle cancel
        modalEl.addEventListener('hidden.bs.modal', function() {
            if (onCancel) onCancel();
            modalEl.remove();
        });
        
        modal.show();
    }

    // Expose API globally
    window.Toast = {
        show: showToast,
        success: showSuccess,
        error: showError,
        warning: showWarning,
        info: showInfo,
        confirm: showConfirm
    };

    console.log('Toast notification system initialized');
})();
