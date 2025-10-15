// Theme Toggle Functionality
const initThemeToggle = () => {
    const themeToggleBtn = document.getElementById('themeToggle');
    const themeToggleIcon = themeToggleBtn.querySelector('i');
    const htmlElement = document.documentElement;

    function setTheme(theme) {
        htmlElement.setAttribute('data-bs-theme', theme);
        if (theme === 'dark') {
            themeToggleIcon.classList.remove('fa-sun');
            themeToggleIcon.classList.add('fa-moon');
        } else {
            themeToggleIcon.classList.remove('fa-moon');
            themeToggleIcon.classList.add('fa-sun');
        }
        
        // Store the theme preference
        localStorage.setItem('theme', theme);
    }

    themeToggleBtn.addEventListener('click', () => {
        const currentTheme = htmlElement.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        setTheme(newTheme);
    });
    
    // Set initial theme based on stored preference or default to dark
    const storedTheme = localStorage.getItem('theme') || 'dark';
    setTheme(storedTheme);
};

// Initialize theme toggle when DOM is loaded
document.addEventListener('DOMContentLoaded', initThemeToggle);