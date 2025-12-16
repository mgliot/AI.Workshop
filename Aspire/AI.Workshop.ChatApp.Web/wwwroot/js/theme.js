// Theme management for AI Workshop Chat
window.themeManager = {
    // Get current theme from localStorage or default to 'dark'
    getTheme: function () {
        return localStorage.getItem('theme') || 'dark';
    },

    // Set theme and persist to localStorage
    setTheme: function (theme) {
        localStorage.setItem('theme', theme);
        document.documentElement.setAttribute('data-theme', theme);
        return theme;
    },

    // Toggle between light and dark
    toggleTheme: function () {
        const current = this.getTheme();
        const newTheme = current === 'dark' ? 'light' : 'dark';
        return this.setTheme(newTheme);
    },

    // Initialize theme on page load
    init: function () {
        const theme = this.getTheme();
        document.documentElement.setAttribute('data-theme', theme);
        return theme;
    }
};

// Initialize theme immediately to prevent flash
(function () {
    const theme = localStorage.getItem('theme') || 'dark';
    document.documentElement.setAttribute('data-theme', theme);
})();
