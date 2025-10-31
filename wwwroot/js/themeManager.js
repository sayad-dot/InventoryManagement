class ThemeManager {
    constructor() {
        this.currentTheme = this.getStoredTheme() || 'light';
        this.currentLanguage = this.getStoredLanguage() || 'en';
        this.init();
    }

    init() {
        this.applyTheme(this.currentTheme);
        this.setupEventListeners();
        this.loadUserPreferences();
    }

    getStoredTheme() {
        return document.documentElement.getAttribute('data-theme') || 
               this.getCookie('theme') || 
               'light';
    }

    getStoredLanguage() {
        return document.documentElement.getAttribute('lang') || 
               this.getCookie('culture')?.split('|')[0]?.split('=')[1] || 
               'en';
    }

    getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
    }

    applyTheme(theme) {
        document.documentElement.setAttribute('data-theme', theme);
        this.currentTheme = theme;
        
        // Update theme toggle button
        const themeToggle = document.getElementById('themeToggle');
        if (themeToggle) {
            const icon = themeToggle.querySelector('i');
            const text = themeToggle.querySelector('.theme-text');
            if (theme === 'dark') {
                if (icon) icon.className = 'fas fa-sun me-1';
                if (text) text.textContent = this.currentLanguage === 'es' ? 'Claro' : 'Light';
            } else {
                if (icon) icon.className = 'fas fa-moon me-1';
                if (text) text.textContent = this.currentLanguage === 'es' ? 'Oscuro' : 'Dark';
            }
        }
    }

    setupEventListeners() {
        // Theme toggle
        document.addEventListener('click', (e) => {
            if (e.target.closest('#themeToggle')) {
                e.preventDefault();
                this.toggleTheme();
            }
        });

        // Language switcher
        document.addEventListener('click', (e) => {
            if (e.target.closest('[data-language]')) {
                e.preventDefault();
                const language = e.target.closest('[data-language]').getAttribute('data-language');
                this.switchLanguage(language);
            }
        });
    }

    toggleTheme() {
        const newTheme = this.currentTheme === 'light' ? 'dark' : 'light';
        this.applyTheme(newTheme);
        this.saveTheme(newTheme);
    }

    switchLanguage(language) {
        this.currentLanguage = language;
        document.documentElement.setAttribute('lang', language);
        this.saveLanguage(language);
        
        // Reload to apply language changes
        window.location.reload();
    }

    saveTheme(theme) {
        // Save via AJAX to server
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        const tokenValue = token ? token.value : '';
        
        fetch('/Localization/SetTheme', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': tokenValue
            },
            body: `theme=${theme}&returnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`
        }).catch(console.error);
    }

    saveLanguage(language) {
        // Save via AJAX to server
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        const tokenValue = token ? token.value : '';
        
        fetch('/Localization/SetLanguage', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': tokenValue
            },
            body: `culture=${language}&returnUrl=${encodeURIComponent(window.location.pathname + window.location.search)}`
        }).catch(console.error);
    }

    loadUserPreferences() {
        // Apply stored preferences on page load
        this.applyTheme(this.currentTheme);
    }
}

// Initialize theme manager when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.themeManager = new ThemeManager();
});