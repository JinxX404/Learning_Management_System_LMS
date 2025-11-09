/**
 * Form Validation Module
 * Provides Bootstrap 5 form validation with custom error handling
 */

(function() {
    'use strict';

    // Fetch all forms with needs-validation class
    const forms = document.querySelectorAll('.needs-validation');

    // Loop over forms and prevent submission if invalid
    Array.from(forms).forEach(form => {
        form.addEventListener('submit', event => {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                
                // Focus first invalid field
                const firstInvalid = form.querySelector(':invalid');
                if (firstInvalid) {
                    firstInvalid.focus();
                    
                    // Scroll to first error if needed
                    firstInvalid.scrollIntoView({ 
                        behavior: 'smooth', 
                        block: 'center' 
                    });
                }
            }
            
            form.classList.add('was-validated');
        }, false);
    });

    // Email validation helper
    function validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(String(email).toLowerCase());
    }

    // Password strength checker
    function checkPasswordStrength(password) {
        let strength = 0;
        
        if (password.length >= 8) strength++;
        if (password.length >= 12) strength++;
        if (/[a-z]/.test(password)) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[^a-zA-Z0-9]/.test(password)) strength++;
        
        return strength;
    }

    // Add password strength indicator
    const passwordInputs = document.querySelectorAll('input[type="password"]');
    passwordInputs.forEach(input => {
        // Skip confirm password fields
        if (input.id.includes('confirm') || input.name.includes('confirm')) {
            return;
        }

        const strengthIndicator = document.createElement('div');
        strengthIndicator.className = 'password-strength mt-2';
        strengthIndicator.innerHTML = `
            <div class="d-flex gap-1">
                <div class="strength-bar flex-fill"></div>
                <div class="strength-bar flex-fill"></div>
                <div class="strength-bar flex-fill"></div>
                <div class="strength-bar flex-fill"></div>
            </div>
            <small class="strength-text text-muted"></small>
        `;
        
        input.parentNode.appendChild(strengthIndicator);
        
        const bars = strengthIndicator.querySelectorAll('.strength-bar');
        const text = strengthIndicator.querySelector('.strength-text');
        
        input.addEventListener('input', function() {
            const strength = checkPasswordStrength(this.value);
            
            // Reset bars
            bars.forEach(bar => {
                bar.style.height = '4px';
                bar.style.backgroundColor = '#dee2e6';
                bar.style.transition = 'all 0.3s ease';
            });
            
            // Update bars based on strength
            if (strength === 0) {
                text.textContent = '';
            } else if (strength <= 2) {
                text.textContent = 'Weak';
                text.className = 'strength-text text-danger';
                bars[0].style.backgroundColor = '#dc3545';
            } else if (strength <= 4) {
                text.textContent = 'Medium';
                text.className = 'strength-text text-warning';
                bars[0].style.backgroundColor = '#ffc107';
                bars[1].style.backgroundColor = '#ffc107';
            } else {
                text.textContent = 'Strong';
                text.className = 'strength-text text-success';
                bars.forEach(bar => bar.style.backgroundColor = '#198754');
            }
        });
    });

    // Confirm password validation
    const confirmPasswordInputs = document.querySelectorAll('input[name*="confirm"], input[id*="confirm"]');
    confirmPasswordInputs.forEach(confirmInput => {
        const passwordInput = confirmInput.form.querySelector('input[type="password"]:not([name*="confirm"]):not([id*="confirm"])');
        
        if (passwordInput) {
            confirmInput.addEventListener('input', function() {
                if (this.value !== passwordInput.value) {
                    this.setCustomValidity('Passwords do not match');
                } else {
                    this.setCustomValidity('');
                }
            });
            
            passwordInput.addEventListener('input', function() {
                if (confirmInput.value && confirmInput.value !== this.value) {
                    confirmInput.setCustomValidity('Passwords do not match');
                } else {
                    confirmInput.setCustomValidity('');
                }
            });
        }
    });

    // Real-time email validation
    const emailInputs = document.querySelectorAll('input[type="email"]');
    emailInputs.forEach(input => {
        input.addEventListener('blur', function() {
            if (this.value && !validateEmail(this.value)) {
                this.setCustomValidity('Please enter a valid email address');
            } else {
                this.setCustomValidity('');
            }
        });
    });

    // File upload validation
    const fileInputs = document.querySelectorAll('input[type="file"]');
    fileInputs.forEach(input => {
        input.addEventListener('change', function() {
            const maxSize = this.dataset.maxSize || 5242880; // 5MB default
            const allowedTypes = this.dataset.allowedTypes?.split(',') || [];
            
            if (this.files.length > 0) {
                const file = this.files[0];
                
                // Check file size
                if (file.size > maxSize) {
                    this.setCustomValidity(`File size must be less than ${maxSize / 1024 / 1024}MB`);
                    return;
                }
                
                // Check file type
                if (allowedTypes.length > 0) {
                    const fileExt = file.name.split('.').pop().toLowerCase();
                    if (!allowedTypes.includes(fileExt)) {
                        this.setCustomValidity(`Only ${allowedTypes.join(', ')} files are allowed`);
                        return;
                    }
                }
                
                this.setCustomValidity('');
            }
        });
    });

    // Prevent form resubmission on page refresh
    if (window.history.replaceState) {
        window.history.replaceState(null, null, window.location.href);
    }

    // Loading state helper
    window.setFormLoading = function(form, isLoading) {
        const submitBtn = form.querySelector('button[type="submit"]');
        if (submitBtn) {
            if (isLoading) {
                submitBtn.disabled = true;
                submitBtn.dataset.originalText = submitBtn.innerHTML;
                submitBtn.innerHTML = `
                    <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                    Loading...
                `;
            } else {
                submitBtn.disabled = false;
                submitBtn.innerHTML = submitBtn.dataset.originalText || 'Submit';
            }
        }
    };

    console.log('Form validation initialized');
})();
