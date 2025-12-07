// NavBar Helper Functions for C# Integration
window.registerNavBarEvents = (dotNetReference) => {
    // Handle outside clicks
    document.addEventListener("click", (e) => {
        const navbar = document.querySelector(".navbar");
        if (navbar && !navbar.contains(e.target)) {
            dotNetReference.invokeMethodAsync("OnOutsideClick");
        }
    });

    // Handle escape key
    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape") {
            dotNetReference.invokeMethodAsync("OnEscapeKey");
        }
    });

    // Handle window resize
    let resizeTimeout;
    window.addEventListener("resize", () => {
        clearTimeout(resizeTimeout);
        resizeTimeout = setTimeout(() => {
            dotNetReference.invokeMethodAsync("OnWindowResize", window.innerWidth);
        }, 100);
    });
};

// Icon Management
class IconManager {
    constructor() {
        this.isInitializing = false;
        this.initializeIcons();
    }

    initializeIcons() {
        if (this.isInitializing) {
            return;
        }

        this.isInitializing = true;

        if (typeof lucide !== "undefined") {
            lucide.createIcons();
        } else {
            setTimeout(() => {
                this.isInitializing = false;
                this.initializeIcons();
            }, 100);
            return;
        }

        this.isInitializing = false;
    }

    reinitialize() {
        this.initializeIcons();
    }
}

// Theme Management
class ThemeManager {
    constructor() {
        this.storageKey = "blazor-template-theme";
        this.init();
    }

    init() {
        // Apply theme as early as possible
        this.applyTheme(this.getTheme());
    }

    getTheme() {
        // Try to get saved preference
        const savedTheme = localStorage.getItem(this.storageKey);
        if (savedTheme) {
            return savedTheme;
        }

        // Fall back to system preference
        if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
            return "dark";
        }

        return "light";
    }

    setTheme(theme) {
        localStorage.setItem(this.storageKey, theme);
        this.applyTheme(theme);
    }

    applyTheme(theme) {
        if (theme === "dark") {
            document.documentElement.classList.add("dark");
        } else {
            document.documentElement.classList.remove("dark");
        }
    }

    toggle() {
        const currentTheme = this.getTheme();
        const newTheme = currentTheme === "dark" ? "light" : "dark";
        this.setTheme(newTheme);
        return newTheme;
    }
}

// Main Application Initializer
class BlazorTemplateApp {
    constructor() {
        this.components = {};
        this.init();
    }

    init() {
        // Initialize components when DOM is ready
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", this.initializeComponents.bind(this));
        } else {
            this.initializeComponents();
        }
    }

    initializeComponents() {
        // Initialize theme manager first (before icons)
        this.components.themeManager = new ThemeManager();

        // Initialize icon manager
        this.components.iconManager = new IconManager();
    }

    reinitializeIcons() {
        this.components.iconManager?.reinitialize();
    }
}

// Initialize app
window.BlazorTemplateApp = new BlazorTemplateApp();

// Global function for icon reinitialization (can be called from Blazor)
window.reinitializeIcons = function () {
    if (window.BlazorTemplateApp) {
        window.BlazorTemplateApp.reinitializeIcons();
    }
};

// Global functions for theme management (can be called from Blazor)
window.initializeTheme = function () {
    // Theme is already initialized in the app constructor
    // This function exists for consistency with Blazor component lifecycle
};

window.toggleTheme = function () {
    if (window.BlazorTemplateApp && window.BlazorTemplateApp.components.themeManager) {
        return window.BlazorTemplateApp.components.themeManager.toggle();
    }
    return "light";
};
