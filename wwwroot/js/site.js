// site.js - Funções específicas do site com melhorias de layout
(function () {
    'use strict';

    // Configurações
    const config = {
        searchBarFixedWidth: 420,
        minSearchWidth: 350,
        maxSearchWidth: 480,
        navbarBreakpoints: {
            xs: 576,
            sm: 768,
            md: 992,
            lg: 1200,
            xl: 1400,
            xxl: 1600
        }
    };

    // Estado da aplicação
    const state = {
        isProfissionalMode: false,
        searchBarWidth: config.searchBarFixedWidth,
        lastWindowWidth: window.innerWidth,
        categorySuggestionsVisible: false,
        activeCategory: null,
        currentScreenSize: ''
    };

    // Inicialização
    function init() {
        initBootstrapComponents();
        initForms();
        initPasswordToggles();
        initImagePreviews();
        initAutoSubmitForms();
        initSmoothScrolling();
        initLazyLoading();
        initBackToTop();
        initToastNotifications();
        initCopyToClipboard();
        initKeyboardShortcuts();
        initNavbarConsistency();
        initUnreadMessagesCounter();
        initServiceFilters();
        initThemeConsistency();
        initSearchBarResizeHandler();
        initModeDetection();
        initLayoutOptimizations(); // Nova função para otimizações de layout
        initCategorySuggestions();

        // Adicionar classe de carregamento
        document.body.classList.add('loaded');
    }

    // ========== FUNÇÃO PARA OTIMIZAR LAYOUT DINAMICAMENTE ==========
    function initLayoutOptimizations() {
        optimizeLayout();
        window.addEventListener('resize', debounce(optimizeLayout, 250));
    }

    function optimizeLayout() {
        const windowWidth = window.innerWidth;
        const windowHeight = window.innerHeight;

        // Ajustar baseado no tamanho da tela
        adjustLayoutForScreenSize(windowWidth, windowHeight);
        adjustContentSpacing(windowWidth);
        optimizeImages(windowWidth);
    }

    function adjustLayoutForScreenSize(width, height) {
        const body = document.body;
        const html = document.documentElement;

        // Remover classes de tamanho antigas
        body.classList.remove('screen-xs', 'screen-sm', 'screen-md', 'screen-lg', 'screen-xl', 'screen-xxl');

        // Determinar tamanho da tela
        let screenSize = '';
        if (width < 576) {
            screenSize = 'xs';
            optimizeForSmallScreens();
        } else if (width < 768) {
            screenSize = 'sm';
            optimizeForSmallScreens();
        } else if (width < 992) {
            screenSize = 'md';
            optimizeForTablet();
        } else if (width < 1200) {
            screenSize = 'lg';
            optimizeForDesktop();
        } else if (width < 1600) {
            screenSize = 'xl';
            optimizeForDesktop();
        } else {
            screenSize = 'xxl';
            optimizeForLargeScreens();
        }

        // Atualizar estado
        state.currentScreenSize = screenSize;
        body.classList.add(`screen-${screenSize}`);

        // Ajustar altura se necessário (para mobile)
        if (height < 600) {
            body.classList.add('short-screen');
            adjustForShortScreen(height);
        } else {
            body.classList.remove('short-screen');
        }
    }

    function adjustContentSpacing(width) {
        const containers = document.querySelectorAll('.container, .container-fluid, .content-wrapper');
        const panels = document.querySelectorAll('.content-panel, .card, .grid-item');

        // Ajustar padding baseado na largura
        let paddingValue;
        if (width < 576) {
            paddingValue = '0.375rem';
        } else if (width < 768) {
            paddingValue = '0.5rem';
        } else if (width < 992) {
            paddingValue = '0.75rem';
        } else if (width < 1200) {
            paddingValue = '1rem';
        } else {
            paddingValue = '1.5rem';
        }

        // Aplicar aos containers
        containers.forEach(container => {
            container.style.paddingLeft = paddingValue;
            container.style.paddingRight = paddingValue;
        });

        // Ajustar espaçamento interno dos painéis em telas pequenas
        if (width < 768) {
            panels.forEach(panel => {
                panel.style.padding = `1rem ${paddingValue}`;
            });
        }
    }

    function optimizeForSmallScreens() {
        // Esconder elementos não essenciais
        const nonEssential = document.querySelectorAll('.non-essential-mobile, .desktop-only');
        nonEssential.forEach(el => {
            el.style.display = 'none';
        });

        // Mostrar elementos mobile
        const mobileElements = document.querySelectorAll('.mobile-only');
        mobileElements.forEach(el => {
            el.style.display = '';
        });

        // Ajustar tamanho de fonte
        document.documentElement.style.fontSize = '14px';

        // Otimizar grid layouts
        const grids = document.querySelectorAll('.grid-layout, .search-results-grid, .category-grid');
        grids.forEach(grid => {
            grid.style.gridTemplateColumns = '1fr';
            grid.style.gap = '0.75rem';
        });
    }

    function optimizeForTablet() {
        // Mostrar elementos para tablet
        const tabletElements = document.querySelectorAll('.tablet-visible');
        tabletElements.forEach(el => {
            el.style.display = '';
        });

        // Ajustar grid para 2 colunas
        const grids = document.querySelectorAll('.grid-layout, .search-results-grid');
        grids.forEach(grid => {
            grid.style.gridTemplateColumns = 'repeat(2, 1fr)';
            grid.style.gap = '1rem';
        });

        // Categorias em 3 colunas
        const categoryGrids = document.querySelectorAll('.category-grid');
        categoryGrids.forEach(grid => {
            grid.style.gridTemplateColumns = 'repeat(3, 1fr)';
        });
    }

    function optimizeForDesktop() {
        // Layout padrão para desktop
        document.documentElement.style.fontSize = '16px';

        const grids = document.querySelectorAll('.grid-layout, .search-results-grid');
        grids.forEach(grid => {
            grid.style.gridTemplateColumns = 'repeat(auto-fill, minmax(300px, 1fr))';
            grid.style.gap = '1.5rem';
        });
    }

    function optimizeForLargeScreens() {
        // Aproveitar espaço extra
        const grids = document.querySelectorAll('.grid-layout, .search-results-grid');
        grids.forEach(grid => {
            grid.style.gridTemplateColumns = 'repeat(auto-fill, minmax(350px, 1fr))';
            grid.style.gap = '2rem';
        });

        // Aumentar um pouco o tamanho da fonte
        document.documentElement.style.fontSize = '17px';
    }

    function adjustForShortScreen(height) {
        // Ajustar elementos para telas curtas
        const tallElements = document.querySelectorAll('.tall-element, .hero-section');
        tallElements.forEach(el => {
            el.style.maxHeight = `${height * 0.8}px`;
            el.style.overflowY = 'auto';
        });
    }

    function optimizeImages(width) {
        // Carregar imagens otimizadas baseadas no tamanho da tela
        const images = document.querySelectorAll('img[data-src-small], img[data-src-medium], img[data-src-large]');

        images.forEach(img => {
            let src;
            if (width < 768) {
                src = img.dataset.srcSmall || img.src;
            } else if (width < 1200) {
                src = img.dataset.srcMedium || img.src;
            } else {
                src = img.dataset.srcLarge || img.src;
            }

            if (src && src !== img.src) {
                img.src = src;
            }
        });
    }

    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // Detectar modo atual
    function initModeDetection() {
        const modeIndicator = document.querySelector('[data-mode-indicator]');
        if (modeIndicator) {
            state.isProfissionalMode = modeIndicator.textContent.includes('Profissional');
            updateLayoutForMode();
        }
    }

    // Inicializar consistência do navbar
    function initNavbarConsistency() {
        const searchContainer = document.querySelector('.search-container');
        if (!searchContainer) return;

        // Adicionar classe para tamanho fixo
        searchContainer.classList.add('fixed-size');

        // Configurar largura inicial
        updateSearchBarWidth();

        // Adicionar observer para mudanças no DOM
        const observer = new MutationObserver(function (mutations) {
            mutations.forEach(function (mutation) {
                if (mutation.type === 'childList') {
                    updateSearchBarWidth();
                }
            });
        });

        observer.observe(searchContainer, { childList: true, subtree: true });
    }

    // Atualizar largura da barra de pesquisa
    function updateSearchBarWidth() {
        const searchContainer = document.querySelector('.search-container.fixed-size');
        if (!searchContainer) return;

        const windowWidth = window.innerWidth;

        // Ajustar baseado no tamanho da tela
        if (windowWidth >= config.navbarBreakpoints.xxl) {
            searchContainer.style.width = `${config.searchBarFixedWidth}px`;
            searchContainer.style.minWidth = `${config.searchBarFixedWidth}px`;
            searchContainer.style.maxWidth = `${config.searchBarFixedWidth}px`;
        } else if (windowWidth >= config.navbarBreakpoints.xl) {
            const width = Math.min(config.searchBarFixedWidth - 40, config.maxSearchWidth);
            searchContainer.style.width = `${width}px`;
            searchContainer.style.minWidth = `${width}px`;
            searchContainer.style.maxWidth = `${width}px`;
        } else if (windowWidth >= config.navbarBreakpoints.lg) {
            const width = Math.min(config.searchBarFixedWidth - 70, config.minSearchWidth);
            searchContainer.style.width = `${width}px`;
            searchContainer.style.minWidth = `${width}px`;
            searchContainer.style.maxWidth = `${width}px`;
        }

        // Forçar repaint para garantir consistência
        searchContainer.style.display = 'none';
        searchContainer.offsetHeight; // Trigger reflow
        searchContainer.style.display = '';
    }

    // Handler para redimensionamento da janela
    function initSearchBarResizeHandler() {
        let resizeTimeout;

        window.addEventListener('resize', function () {
            clearTimeout(resizeTimeout);
            resizeTimeout = setTimeout(function () {
                updateSearchBarWidth();
                updateLayoutForMode();
                optimizeNavbarLayout();
            }, 250);
        });
    }

    // Otimizar layout do navbar
    function optimizeNavbarLayout() {
        const navbarContent = document.querySelector('.navbar-content');
        if (!navbarContent) return;

        const availableWidth = navbarContent.offsetWidth;
        const logo = document.querySelector('.logo-container');
        const nav = document.querySelector('.main-nav');
        const search = document.querySelector('.search-container');
        const userMenu = document.querySelector('.user-menu');

        if (!logo || !nav || !search || !userMenu) return;

        // Calcular larguras
        const logoWidth = logo.offsetWidth;
        const navWidth = nav.offsetWidth;
        const searchWidth = search.offsetWidth;
        const userMenuWidth = userMenu.offsetWidth;
        const totalWidth = logoWidth + navWidth + searchWidth + userMenuWidth + 120; // Margens e gaps

        // Ajustar se estiver muito apertado
        if (totalWidth > availableWidth) {
            // Reduzir padding dos itens de navegação
            const navItems = nav.querySelectorAll('.nav-link');
            navItems.forEach(item => {
                item.style.padding = '0.5rem 0.5rem !important';
                item.style.fontSize = '0.85rem';
            });

            // Ajustar largura da barra de pesquisa
            if (searchWidth > 300) {
                search.style.width = '300px';
                search.style.minWidth = '300px';
                search.style.maxWidth = '300px';
            }
        } else {
            // Restaurar estilos padrão
            const navItems = nav.querySelectorAll('.nav-link');
            navItems.forEach(item => {
                item.style.padding = '';
                item.style.fontSize = '';
            });

            // Restaurar largura da barra de pesquisa
            updateSearchBarWidth();
        }
    }

    // Atualizar layout baseado no modo
    function updateLayoutForMode() {
        const searchContainer = document.querySelector('.search-container');
        const navLinks = document.querySelectorAll('.nav-link');

        if (!searchContainer) return;

        // Garantir que a barra de pesquisa mantenha o mesmo tamanho
        searchContainer.style.transition = 'none';

        // Ajustar itens de navegação se necessário
        if (state.isProfissionalMode) {
            // Adicionar classe específica para modo profissional
            document.body.classList.add('mode-profissional');
            document.body.classList.remove('mode-cliente');

            // Ajustar estilos dos links se necessário
            navLinks.forEach(link => {
                if (link.textContent.includes('Meus Serviços')) {
                    link.classList.add('nav-link-profissional');
                }
            });
        } else {
            // Modo cliente
            document.body.classList.add('mode-cliente');
            document.body.classList.remove('mode-profissional');

            // Remover estilos específicos
            navLinks.forEach(link => {
                link.classList.remove('nav-link-profissional');
            });
        }

        // Forçar repaint para consistência
        searchContainer.offsetHeight;
    }

    // ========== NOVA FUNÇÃO: INICIALIZAR SUGESTÕES DE CATEGORIA ==========
    function initCategorySuggestions() {
        const searchInput = document.querySelector('.search-bar');
        if (!searchInput) return;

        // Criar container de sugestões
        const suggestionsContainer = document.createElement('div');
        suggestionsContainer.className = 'category-suggestions';
        suggestionsContainer.id = 'categorySuggestions';

        // Conteúdo das sugestões
        const categories = [
            { id: 'eletricista', name: 'Eletricista', icon: 'bi-lightning-charge' },
            { id: 'encanador', name: 'Encanador', icon: 'bi-droplet' },
            { id: 'pintor', name: 'Pintor', icon: 'bi-brush' },
            { id: 'pedreiro', name: 'Pedreiro', icon: 'bi-hammer' },
            { id: 'jardineiro', name: 'Jardineiro', icon: 'bi-flower1' },
            { id: 'limpeza', name: 'Limpeza', icon: 'bi-broom' },
            { id: 'informatica', name: 'Informática', icon: 'bi-laptop' },
            { id: 'design', name: 'Design Gráfico', icon: 'bi-palette' },
            { id: 'advogado', name: 'Advogado', icon: 'bi-scale' },
            { id: 'contador', name: 'Contador', icon: 'bi-calculator' },
            { id: 'professor', name: 'Professor', icon: 'bi-book' },
            { id: 'cozinheiro', name: 'Cozinheiro', icon: 'bi-egg-fried' }
        ];

        // Montar HTML das sugestões
        suggestionsContainer.innerHTML = `
            <div class="category-suggestions-header">
                <h6>Categorias Populares</h6>
                <button class="category-suggestions-close" onclick="hideCategorySuggestions()">
                    <i class="bi bi-x-lg"></i>
                </button>
            </div>
            <div class="category-grid">
                ${categories.map(category => `
                    <a href="/Home/Procurar?categoria=${category.id}" 
                       class="category-chip" 
                       data-category="${category.id}"
                       onclick="selectCategory('${category.id}', '${category.name}')">
                        <i class="bi ${category.icon}"></i>
                        <span>${category.name}</span>
                    </a>
                `).join('')}
            </div>
            <div class="category-suggestions-footer">
                <a href="/Home/Procurar" class="category-search-link">
                    <i class="bi bi-search"></i>
                    Ver todas as categorias
                </a>
                <button class="btn btn-sm btn-outline-primary" onclick="hideCategorySuggestions()">
                    Fechar
                </button>
            </div>
        `;

        // Adicionar ao DOM
        searchInput.parentElement.appendChild(suggestionsContainer);

        // Event listeners
        searchInput.addEventListener('focus', showCategorySuggestions);
        searchInput.addEventListener('input', handleSearchInput);

        // Fechar sugestões ao clicar fora
        document.addEventListener('click', function (event) {
            if (!suggestionsContainer.contains(event.target) &&
                !searchInput.contains(event.target)) {
                hideCategorySuggestions();
            }
        });

        // Fechar com ESC
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape' && state.categorySuggestionsVisible) {
                hideCategorySuggestions();
            }
        });

        // Melhorar acessibilidade com teclado
        searchInput.addEventListener('keydown', function (event) {
            if (event.key === 'ArrowDown' && state.categorySuggestionsVisible) {
                event.preventDefault();
                const firstChip = suggestionsContainer.querySelector('.category-chip');
                if (firstChip) firstChip.focus();
            }
        });
    }

    // Mostrar sugestões de categoria
    function showCategorySuggestions() {
        const suggestions = document.getElementById('categorySuggestions');
        if (!suggestions) return;

        suggestions.classList.add('show');
        state.categorySuggestionsVisible = true;

        // Posicionar corretamente
        const searchInput = document.querySelector('.search-bar');
        const rect = searchInput.getBoundingClientRect();
        suggestions.style.top = `${rect.bottom + window.scrollY}px`;
        suggestions.style.left = `${rect.left}px`;
        suggestions.style.width = `${rect.width}px`;

        // Adicionar classe para indicar que está aberto
        searchInput.parentElement.classList.add('suggestions-open');
    }

    // Esconder sugestões de categoria
    function hideCategorySuggestions() {
        const suggestions = document.getElementById('categorySuggestions');
        if (!suggestions) return;

        suggestions.classList.remove('show');
        state.categorySuggestionsVisible = false;

        // Remover classe
        const searchInput = document.querySelector('.search-bar');
        if (searchInput) {
            searchInput.parentElement.classList.remove('suggestions-open');
        }
    }

    // Manipular input de pesquisa
    function handleSearchInput(event) {
        const value = event.target.value.trim();
        if (value.length > 0) {
            // Filtrar categorias baseado no input
            filterCategories(value);
        } else {
            // Mostrar todas as categorias
            resetCategoryFilter();
        }
    }

    // Filtrar categorias
    function filterCategories(searchTerm) {
        const chips = document.querySelectorAll('.category-chip');
        const lowerSearchTerm = searchTerm.toLowerCase();

        chips.forEach(chip => {
            const categoryName = chip.querySelector('span').textContent.toLowerCase();
            if (categoryName.includes(lowerSearchTerm)) {
                chip.style.display = 'flex';
                chip.classList.add('category-chip-highlight');
            } else {
                chip.style.display = 'none';
                chip.classList.remove('category-chip-highlight');
            }
        });
    }

    // Resetar filtro de categorias
    function resetCategoryFilter() {
        const chips = document.querySelectorAll('.category-chip');
        chips.forEach(chip => {
            chip.style.display = 'flex';
            chip.classList.remove('category-chip-highlight');
        });
    }

    // Selecionar categoria
    function selectCategory(categoryId, categoryName) {
        const searchInput = document.querySelector('.search-bar');
        if (searchInput) {
            searchInput.value = categoryName;
            searchInput.focus();

            // Destacar a categoria selecionada
            const chips = document.querySelectorAll('.category-chip');
            chips.forEach(chip => {
                chip.classList.remove('category-chip-active');
                if (chip.dataset.category === categoryId) {
                    chip.classList.add('category-chip-active');
                }
            });

            // Atualizar estado
            state.activeCategory = categoryId;

            // Fechar sugestões após 1 segundo
            setTimeout(hideCategorySuggestions, 1000);
        }
    }

    // ========== FUNÇÕES EXISTENTES (mantidas para compatibilidade) ==========

    // Inicializar componentes Bootstrap
    function initBootstrapComponents() {
        // Tooltips
        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl, {
                trigger: 'hover focus'
            });
        });

        // Popovers
        const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.map(function (popoverTriggerEl) {
            return new bootstrap.Popover(popoverTriggerEl, {
                trigger: 'focus'
            });
        });

        // Modals
        const modals = document.querySelectorAll('.modal');
        modals.forEach(modal => {
            modal.addEventListener('show.bs.modal', function () {
                document.body.style.overflow = 'hidden';
            });

            modal.addEventListener('hidden.bs.modal', function () {
                document.body.style.overflow = '';
            });
        });
    }

    // Inicializar formulários
    function initForms() {
        document.querySelectorAll('.needs-validation').forEach(form => {
            form.addEventListener('submit', function (event) {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }

    // Alternar visibilidade da senha
    function initPasswordToggles() {
        document.querySelectorAll('.password-toggle').forEach(toggle => {
            toggle.addEventListener('click', function () {
                const input = this.parentElement.querySelector('input');
                if (input.type === 'password') {
                    input.type = 'text';
                } else {
                    input.type = 'password';
                }
            });
        });
    }

    // Preview de imagens
    function initImagePreviews() {
        document.querySelectorAll('input[type="file"][accept^="image"]').forEach(input => {
            input.addEventListener('change', function () {
                const previewId = this.dataset.preview;
                if (!previewId) return;

                const preview = document.getElementById(previewId);
                if (!preview) return;

                if (this.files && this.files[0]) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        preview.src = e.target.result;
                        preview.style.display = 'block';
                    };
                    reader.readAsDataURL(this.files[0]);
                }
            });
        });
    }

    // Auto-submit de formulários
    function initAutoSubmitForms() {
        document.querySelectorAll('form[data-auto-submit]').forEach(form => {
            form.querySelectorAll('input, select, textarea').forEach(input => {
                input.addEventListener('change', () => {
                    setTimeout(() => form.submit(), 100);
                });
            });
        });
    }

    // Scroll suave
    function initSmoothScrolling() {
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                e.preventDefault();
                const targetId = this.getAttribute('href');
                if (targetId === '#' || targetId === '#!') return;

                const targetElement = document.querySelector(targetId);
                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
    }

    // Lazy loading de imagens
    function initLazyLoading() {
        if ('IntersectionObserver' in window) {
            const lazyImages = document.querySelectorAll('img[data-src]');
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        img.classList.add('loaded');
                        imageObserver.unobserve(img);
                    }
                });
            });
            lazyImages.forEach(img => imageObserver.observe(img));
        }
    }

    // Botão "voltar ao topo"
    function initBackToTop() {
        const backToTopBtn = document.createElement('button');
        backToTopBtn.className = 'btn btn-primary back-to-top';
        backToTopBtn.innerHTML = '<i class="bi bi-chevron-up"></i>';
        backToTopBtn.setAttribute('aria-label', 'Voltar ao topo');
        backToTopBtn.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            z-index: 1000;
            display: none;
            width: 45px;
            height: 45px;
            border-radius: 50%;
            padding: 0;
        `;

        backToTopBtn.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });

        document.body.appendChild(backToTopBtn);

        window.addEventListener('scroll', () => {
            backToTopBtn.style.display = window.pageYOffset > 300 ? 'flex' : 'none';
        });
    }

    // Notificações toast
    function initToastNotifications() {
        document.querySelectorAll('.toast').forEach(toastEl => {
            const toast = new bootstrap.Toast(toastEl, {
                autohide: true,
                delay: 5000
            });
            toast.show();
        });

        window.showToast = function (options) {
            const { title, message, type = 'info', duration = 5000 } = options;
            const toastId = 'toast-' + Date.now();
            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white bg-${type}" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">
                            ${title ? `<strong>${title}</strong><br>` : ''}
                            ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;

            let container = document.querySelector('.toast-container');
            if (!container) {
                container = document.createElement('div');
                container.className = 'toast-container position-fixed top-0 end-0 p-3';
                container.style.zIndex = '1090';
                document.body.appendChild(container);
            }

            container.insertAdjacentHTML('beforeend', toastHtml);
            const toastEl = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastEl, { autohide: true, delay: duration });
            toast.show();

            toastEl.addEventListener('hidden.bs.toast', () => {
                toastEl.remove();
            });

            return toast;
        };
    }

    // Copiar para clipboard
    function initCopyToClipboard() {
        document.querySelectorAll('[data-copy]').forEach(btn => {
            btn.addEventListener('click', async function () {
                const textToCopy = this.dataset.copy;
                try {
                    await navigator.clipboard.writeText(textToCopy);
                    const originalText = this.innerHTML;
                    this.innerHTML = '<i class="bi bi-check"></i> Copiado!';
                    setTimeout(() => {
                        this.innerHTML = originalText;
                    }, 2000);
                } catch (err) {
                    console.error('Erro ao copiar:', err);
                }
            });
        });
    }

    // Eventos de teclado
    function initKeyboardShortcuts() {
        document.addEventListener('keydown', function (e) {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.querySelector('.search-bar');
                if (searchInput) {
                    searchInput.focus();
                    showCategorySuggestions();
                }
            }
            if (e.key === 'Escape') {
                const sidebar = document.getElementById('githubSidebar');
                if (sidebar && sidebar.classList.contains('active')) {
                    window.toggleSidebar();
                }
                if (state.categorySuggestionsVisible) {
                    hideCategorySuggestions();
                }
            }
        });
    }

    // Contador de mensagens não lidas
    function initUnreadMessagesCounter() {
        function updateUnreadCount() {
            const isAuthenticated = document.body.getAttribute('data-user-authenticated') === 'True';
            if (!isAuthenticated) return;

            fetch('/Dashboard/GetUnreadCount')
                .then(response => response.json())
                .then(data => {
                    const count = data.count || 0;
                    const sidebarBadge = document.getElementById('sidebarMessageBadge');
                    const headerBadge = document.getElementById('headerMessageBadge');

                    if (count > 0) {
                        if (sidebarBadge) {
                            sidebarBadge.textContent = count;
                            sidebarBadge.style.display = 'inline-block';
                        }
                        if (headerBadge) {
                            headerBadge.textContent = count;
                            headerBadge.style.display = 'inline-block';
                        }
                    } else {
                        if (sidebarBadge) {
                            sidebarBadge.textContent = '';
                            sidebarBadge.style.display = 'none';
                        }
                        if (headerBadge) {
                            headerBadge.textContent = '';
                            headerBadge.style.display = 'none';
                        }
                    }
                })
                .catch(error => console.error('Erro ao carregar contador de mensagens:', error));
        }

        const isAuthenticated = document.body.getAttribute('data-user-authenticated') === 'True';
        if (isAuthenticated) {
            updateUnreadCount();
            setInterval(updateUnreadCount, 30000);
        }
    }

    // Filtros de serviços
    function initServiceFilters() {
        const estadoFilter = document.getElementById('estadoFilter');
        if (estadoFilter) {
            estadoFilter.addEventListener('change', function () {
                const estado = this.value;
                document.querySelectorAll('#servicosTable tbody tr').forEach(row => {
                    row.style.display = !estado || row.dataset.estado === estado ? '' : 'none';
                });
            });
        }

        const ordenarPor = document.getElementById('ordenarPor');
        if (ordenarPor) {
            ordenarPor.addEventListener('change', function () {
                const tbody = document.querySelector('#servicosTable tbody');
                const rows = Array.from(tbody.querySelectorAll('tr:not([style*="none"])'));
                rows.sort((a, b) => {
                    switch (this.value) {
                        case 'dataDesc': return new Date(b.dataset.data) - new Date(a.dataset.data);
                        case 'dataAsc': return new Date(a.dataset.data) - new Date(b.dataset.data);
                        case 'precoDesc': return parseFloat(b.dataset.preco) - parseFloat(a.dataset.preco);
                        case 'precoAsc': return parseFloat(a.dataset.preco) - parseFloat(b.dataset.preco);
                        case 'titulo': return a.dataset.titulo.localeCompare(b.dataset.titulo);
                        default: return 0;
                    }
                });
                rows.forEach(row => tbody.appendChild(row));
            });
        }
    }

    // Consistência do tema
    function initThemeConsistency() {
        const savedTheme = localStorage.getItem('theme');
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

        if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
            document.documentElement.setAttribute('data-theme', 'dark');
        } else {
            document.documentElement.setAttribute('data-theme', 'light');
        }

        setTimeout(() => {
            document.documentElement.classList.add('theme-transition');
        }, 100);
    }

    // Funções globais
    window.getEstadoBadgeClass = function (estado) {
        switch (estado) {
            case 'Pendente': return 'badge-pendente';
            case 'Aceite': return 'badge-aceite';
            case 'EmProgresso': return 'badge-emprogresso';
            case 'Concluido': return 'badge-concluido';
            case 'Cancelado': return 'badge-cancelado';
            default: return 'badge-secondary';
        }
    };

    window.confirmAction = function (message) {
        return confirm(message || 'Tem a certeza que pretende realizar esta ação?');
    };

    window.openCancelModal = function (servicoId, servicoTitulo) {
        const modal = new bootstrap.Modal(document.getElementById('cancelarModal'));
        document.getElementById('servicoId').value = servicoId;
        document.getElementById('servicoTitulo').textContent = servicoTitulo;
        modal.show();
    };

    window.goToProcurar = function () {
        window.location.href = '/Home/Procurar';
    };

    window.toggleSidebar = function () {
        const sidebar = document.getElementById('githubSidebar');
        const overlay = document.getElementById('sidebarOverlay');
        sidebar.classList.toggle('active');
        overlay.classList.toggle('active');
        document.body.style.overflow = sidebar.classList.contains('active') ? 'hidden' : '';
    };

    // ========== NOVAS FUNÇÕES GLOBAIS PARA SUGESTÕES DE CATEGORIA ==========
    window.showCategorySuggestions = showCategorySuggestions;
    window.hideCategorySuggestions = hideCategorySuggestions;
    window.selectCategory = selectCategory;
    window.filterCategories = filterCategories;

    // Utilitários
    window.EDSG = {
        formatDate: function (dateString) {
            return new Date(dateString).toLocaleDateString('pt-PT', {
                day: '2-digit',
                month: '2-digit',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });
        },
        formatCurrency: function (amount) {
            return new Intl.NumberFormat('pt-PT', {
                style: 'currency',
                currency: 'EUR'
            }).format(amount);
        },
        truncateText: function (text, maxLength) {
            return text.length <= maxLength ? text : text.substr(0, maxLength) + '...';
        },
        showCategorySuggestions: showCategorySuggestions,
        hideCategorySuggestions: hideCategorySuggestions,
        getCurrentScreenSize: function () {
            return state.currentScreenSize;
        }
    };

    // Auto-dismiss alerts
    setTimeout(() => {
        document.querySelectorAll('.alert:not(.alert-permanent)').forEach(alert => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            bsAlert.close();
        });
    }, 5000);

    // Search on Enter
    const searchBar = document.querySelector('.search-bar');
    if (searchBar) {
        searchBar.addEventListener('keypress', function (e) {
            if (e.key === 'Enter') {
                const query = this.value.trim();
                if (query) {
                    this.closest('form').submit();
                    hideCategorySuggestions();
                }
            }
        });
    }

    // Close sidebar on escape
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && document.getElementById('githubSidebar').classList.contains('active')) {
            window.toggleSidebar();
        }
    });

    // Inicializar quando o DOM estiver pronto
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // API pública
    window.App = {
        initNavbarConsistency: initNavbarConsistency,
        updateSearchBarWidth: updateSearchBarWidth,
        optimizeNavbarLayout: optimizeNavbarLayout,
        showCategorySuggestions: showCategorySuggestions,
        hideCategorySuggestions: hideCategorySuggestions,
        optimizeLayout: optimizeLayout,
        getCurrentScreenSize: function () { return state.currentScreenSize; }
    };

})();

// Funções globais para compatibilidade
function updateUnreadCount() {
    if (document.body.getAttribute('data-user-authenticated') !== 'True') return;

    fetch('/Dashboard/GetUnreadCount')
        .then(response => response.json())
        .then(data => {
            const count = data.count || 0;
            const sidebarBadge = document.getElementById('sidebarMessageBadge');
            const headerBadge = document.getElementById('headerMessageBadge');

            if (count > 0) {
                if (sidebarBadge) sidebarBadge.textContent = count;
                if (headerBadge) headerBadge.textContent = count;
            } else {
                if (sidebarBadge) sidebarBadge.textContent = '';
                if (headerBadge) headerBadge.textContent = '';
            }
        })
        .catch(error => console.error('Erro ao carregar contador de mensagens:', error));
}

// Inicializar contador
if (typeof updateUnreadCount === 'function') {
    document.addEventListener('DOMContentLoaded', function () {
        setTimeout(updateUnreadCount, 1000);
        const isAuthenticated = document.body.getAttribute('data-user-authenticated') === 'True';
        if (isAuthenticated) {
            setInterval(updateUnreadCount, 30000);
        }
    });
}