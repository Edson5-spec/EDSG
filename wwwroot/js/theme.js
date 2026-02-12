// theme.js - Sistema de tema claro/escuro aprimorado
(function () {
    'use strict';

    // Configurações
    const THEME_KEY = 'theme';
    const THEME_TRANSITION_CLASS = 'theme-transition';
    const TRANSITION_DURATION = 300; // ms

    // Elementos do DOM
    let themeToggleBtn = null;

    // Inicialização
    function init() {
        setupTheme();
        setupEventListeners();
        setupSystemThemeListener();
        setupSearchBarTheme(); // Nova função para barra de pesquisa
        updateThemeIcons();
    }

    // Configurar tema inicial
    function setupTheme() {
        const savedTheme = localStorage.getItem(THEME_KEY);
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

        if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
            setTheme('dark');
        } else {
            setTheme('light');
        }

        // Adicionar classe de transição após um pequeno delay
        setTimeout(() => {
            document.documentElement.classList.add(THEME_TRANSITION_CLASS);
        }, 100);
    }

    // Configurar event listeners
    function setupEventListeners() {
        themeToggleBtn = document.querySelector('.theme-toggle-btn');

        if (themeToggleBtn) {
            themeToggleBtn.addEventListener('click', toggleTheme);
            updateToggleButton();
        }

        // Prevenir transições durante a animação
        document.addEventListener('transitionstart', function (e) {
            if (e.propertyName.includes('color') || e.propertyName.includes('background')) {
                document.documentElement.classList.add(THEME_TRANSITION_CLASS);
            }
        });

        document.addEventListener('transitionend', function (e) {
            if (e.propertyName.includes('color') || e.propertyName.includes('background')) {
                setTimeout(() => {
                    document.documentElement.classList.remove(THEME_TRANSITION_CLASS);
                }, TRANSITION_DURATION);
            }
        });
    }

    // Configurar listener para mudanças no tema do sistema
    function setupSystemThemeListener() {
        const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

        mediaQuery.addEventListener('change', e => {
            if (!localStorage.getItem(THEME_KEY)) {
                setTheme(e.matches ? 'dark' : 'light');
                updateToggleButton();
                updateThemeIcons();
                updateSearchBarTheme(); // Atualizar barra de pesquisa
            }
        });
    }

    // Configurar tema da barra de pesquisa
    function setupSearchBarTheme() {
        const searchBars = document.querySelectorAll('.search-bar');
        searchBars.forEach(searchBar => {
            updateSearchBarAppearance(searchBar);
        });
    }

    // Atualizar aparência da barra de pesquisa
    function updateSearchBarAppearance(searchBar) {
        const theme = document.documentElement.getAttribute('data-theme');

        if (theme === 'dark') {
            // Modo escuro: fundo escuro, texto branco
            searchBar.style.background = 'rgba(255, 255, 255, 0.1)';
            searchBar.style.borderColor = 'rgba(255, 255, 255, 0.2)';
            searchBar.style.color = '#ffffff';
            searchBar.style.boxShadow = 'none';
        } else {
            // Modo claro: fundo branco, texto escuro
            searchBar.style.background = 'white';
            searchBar.style.borderColor = 'rgba(0, 0, 0, 0.1)';
            searchBar.style.color = '#111827';
            searchBar.style.boxShadow = 'var(--shadow-sm)';
        }
    }

    // Atualizar tema da barra de pesquisa
    function updateSearchBarTheme() {
        setTimeout(() => {
            const searchBars = document.querySelectorAll('.search-bar');
            searchBars.forEach(searchBar => {
                updateSearchBarAppearance(searchBar);
            });
        }, 10);
    }

    // Definir tema
    function setTheme(theme) {
        const oldTheme = document.documentElement.getAttribute('data-theme');

        if (oldTheme === theme) return;

        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(THEME_KEY, theme);

        // Disparar evento personalizado
        const event = new CustomEvent('themechange', {
            detail: { theme: theme, oldTheme: oldTheme }
        });
        document.documentElement.dispatchEvent(event);

        updateToggleButton();
        updateThemeIcons();
        updateMetaThemeColor(theme);
        updateSearchBarTheme(); // Atualizar barra de pesquisa
    }

    // Atualizar cor do meta theme-color para PWA
    function updateMetaThemeColor(theme) {
        let metaThemeColor = document.querySelector('meta[name="theme-color"]');

        if (!metaThemeColor) {
            metaThemeColor = document.createElement('meta');
            metaThemeColor.name = 'theme-color';
            document.head.appendChild(metaThemeColor);
        }

        // Cores baseadas no tema
        const colors = {
            light: '#1f2937',
            dark: '#0f172a'
        };

        metaThemeColor.content = colors[theme] || colors.light;
    }

    // Alternar tema
    function toggleTheme() {
        const currentTheme = document.documentElement.getAttribute('data-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';

        // Animar o botão antes de mudar o tema
        if (themeToggleBtn) {
            themeToggleBtn.style.transform = 'rotate(180deg) scale(0.9)';
            themeToggleBtn.style.opacity = '0.8';

            setTimeout(() => {
                setTheme(newTheme);

                // Restaurar botão
                setTimeout(() => {
                    themeToggleBtn.style.transform = 'rotate(0deg) scale(1)';
                    themeToggleBtn.style.opacity = '1';
                }, 150);
            }, 150);
        } else {
            setTheme(newTheme);
        }
    }

    // Atualizar botão de toggle
    function updateToggleButton() {
        if (!themeToggleBtn) return;

        const currentTheme = document.documentElement.getAttribute('data-theme');
        const isDark = currentTheme === 'dark';

        // Atualizar título e aria-label
        themeToggleBtn.setAttribute('title', isDark ? 'Mudar para tema claro' : 'Mudar para tema escuro');
        themeToggleBtn.setAttribute('aria-label', isDark ? 'Mudar para tema claro' : 'Mudar para tema escuro');
        themeToggleBtn.setAttribute('aria-pressed', isDark);

        // Adicionar efeito de brilho
        themeToggleBtn.style.boxShadow = isDark ?
            '0 0 0 1px rgba(88, 166, 255, 0.4)' :
            '0 0 0 1px rgba(31, 111, 235, 0.4)';
    }

    // Atualizar ícones do tema
    function updateThemeIcons() {
        const theme = document.documentElement.getAttribute('data-theme');
        const sunIcon = document.getElementById('sun-icon');
        const moonIcon = document.getElementById('moon-icon');

        if (theme === 'dark') {
            if (sunIcon) sunIcon.classList.remove('d-none');
            if (moonIcon) moonIcon.classList.add('d-none');
        } else {
            if (sunIcon) sunIcon.classList.add('d-none');
            if (moonIcon) moonIcon.classList.remove('d-none');
        }
    }

    // Obter tema atual
    function getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') || 'light';
    }

    // Forçar tema (útil para testes)
    function forceTheme(theme) {
        localStorage.setItem(THEME_KEY, theme);
        setTheme(theme);
    }

    // Resetar para tema do sistema
    function resetToSystemTheme() {
        localStorage.removeItem(THEME_KEY);
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        setTheme(prefersDark ? 'dark' : 'light');
    }

    // Inicializar quando o DOM estiver pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // API pública
    window.ThemeManager = {
        toggle: toggleTheme,
        set: setTheme,
        get: getCurrentTheme,
        force: forceTheme,
        reset: resetToSystemTheme
    };

})();