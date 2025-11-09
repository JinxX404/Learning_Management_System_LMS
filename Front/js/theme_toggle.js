/**
 * Theme Toggle System
 * Manages dark/light theme with localStorage persistence across all pages
 * Compatible with Bootstrap 5.3+ data-bs-theme attribute system
 */

// Initialize theme immediately on page load (before DOM renders)
(function initTheme() {
  // Get saved theme from localStorage, or check system preference
  const savedTheme = localStorage.getItem('theme');
  const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  
  // Determine which theme to apply
  const theme = savedTheme || (systemPrefersDark ? 'dark' : 'light');
  
  // Apply theme to html element using Bootstrap 5.3+ data-bs-theme attribute
  document.documentElement.setAttribute('data-bs-theme', theme);
})();

/**
 * Toggle between dark and light themes
 * Saves preference to localStorage
 */
function toggleTheme() {
  const htmlElement = document.documentElement;
  const currentTheme = htmlElement.getAttribute('data-bs-theme');
  const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
  
  htmlElement.setAttribute('data-bs-theme', newTheme);
  localStorage.setItem('theme', newTheme);
}

/**
 * Get current theme
 * @returns {string} 'dark' or 'light'
 */
function getCurrentTheme() {
  return document.documentElement.getAttribute('data-bs-theme') || 'light';
}

/**
 * Set theme explicitly
 * @param {string} theme - 'dark' or 'light'
 */
function setTheme(theme) {
  const htmlElement = document.documentElement;
  htmlElement.setAttribute('data-bs-theme', theme);
  localStorage.setItem('theme', theme);
}

/**
 * Set theme to follow system preference
 * Removes manual preference and syncs with OS settings
 */
function setSystemTheme() {
  localStorage.removeItem('theme');
  const systemPrefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
  const theme = systemPrefersDark ? 'dark' : 'light';
  document.documentElement.setAttribute('data-bs-theme', theme);
}

// Listen for system theme changes (optional - updates theme if user changes system preference)
window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
  // Only update if user hasn't set a manual preference
  if (!localStorage.getItem('theme')) {
    const theme = e.matches ? 'dark' : 'light';
    document.documentElement.setAttribute('data-bs-theme', theme);
  }
});
