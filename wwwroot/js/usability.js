// Usability enhancements for EDSG

class EDSGUsability {
    constructor() {
        this.init();
    }

    init() {
        this.setupFormEnhancements();
        this.setupNavigationEnhancements();
        this.setupFeedbackEnhancements();
        this.setupAccessibility();
        this.setupPerformance();
    }

    setupFormEnhancements() {
        // Auto-focus first input in forms
        document.querySelectorAll('form').forEach(form => {
            const firstInput = form.querySelector('input[type="text"], input[type="email"], input[type="password"]');
            if (firstInput && !firstInput.value) {
                setTimeout(() => firstInput.focus(), 100);
            }
        });

        // Password visibility toggle
        document.querySelectorAll('.password-toggle').forEach(toggle => {
            toggle.addEventListener('click', function () {
                const input = this.previousElementSibling;
                const icon = this.querySelector('i');
                if (input.type === 'password') {
                    input.type = 'text';
                    icon.className = 'bi bi-eye-slash';
                } else {
                    input.type = 'password';
                    icon.className = 'bi bi-eye';
                }
            });
        });

        // Form validation feedback
        document.querySelectorAll('.form-control').forEach(input => {
            input.addEventListener('blur', function () {
                if (this.value.trim() !== '') {
                    this.classList.add('filled');
                } else {
                    this.classList.remove('filled');
                }
            });

            // Real-time validation
            if (input.hasAttribute('pattern')) {
                input.addEventListener('input', function () {
                    const isValid = this.validity.valid;
                    this.classList.toggle('is-valid', isValid && this.value.length > 0);
                    this.classList.toggle('is-invalid', !isValid && this.value.length > 0);
                });
            }
        });
    }

    setupNavigationEnhancements() {
        // Smooth scrolling for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function (e) {
                const targetId = this.getAttribute('href');
                if (targetId !== '#') {
                    e.preventDefault();
                    const targetElement = document.querySelector(targetId);
                    if (targetElement) {
                        targetElement.scrollIntoView({
                            behavior: 'smooth',
                            block: 'start'
                        });
                    }
                }
            });
        });

        // Active nav link highlighting
        const currentPath = window.location.pathname;
        document.querySelectorAll('.nav-link').forEach(link => {
            const linkPath = link.getAttribute('href');
            if (linkPath && currentPath.includes(linkPath) && linkPath !== '/') {
                link.classList.add('active');
                link.setAttribute('aria-current', 'page');
            }
        });

        // Back to top button
        const backToTop = document.createElement('button');
        backToTop.className = 'btn btn-primary back-to-top';
        backToTop.innerHTML = '<i class="bi bi-chevron-up"></i>';
        backToTop.style.cssText = `
            position: fixed;
            bottom: 20px;
            right: 20px;
            display: none;
            z-index: 1000;
            width: 50px;
            height: 50px;
            border-radius: 50%;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
        document.body.appendChild(backToTop);

        window.addEventListener('scroll', () => {
            if (window.pageYOffset > 300) {
                backToTop.style.display = 'block';
                backToTop.classList.add('fade-in');
            } else {
                backToTop.style.display = 'none';
            }
        });

        backToTop.addEventListener('click', () => {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });

        // Quick search shortcut (Ctrl+K)
        document.addEventListener('keydown', (e) => {
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.querySelector('input[type="search"], input[name="search"]');
                if (searchInput) {
                    searchInput.focus();
                } else {
                    window.location.href = '/Home/Procurar';
                }
            }
        });
    }

    setupFeedbackEnhancements() {
        // Toast notifications
        window.showToast = function (message, type = 'info') {
            const toastContainer = document.getElementById('toast-container') || this.createToastContainer();
            const toast = document.createElement('div');
            toast.className = `toast show bg-${type} text-white`;
            toast.innerHTML = `
                <div class="toast-body">
                    <div class="d-flex align-items-center">
                        <i class="bi ${this.getToastIcon(type)} me-2"></i>
                        <div>${message}</div>
                        <button type="button" class="btn-close btn-close-white ms-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;
            toastContainer.appendChild(toast);

            // Auto-remove after 5 seconds
            setTimeout(() => {
                toast.remove();
            }, 5000);
        };

        // Loading indicators
        window.showLoading = function (element) {
            const loading = document.createElement('div');
            loading.className = 'loading-overlay';
            loading.innerHTML = `
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            `;
            element.style.position = 'relative';
            element.appendChild(loading);
        };

        window.hideLoading = function (element) {
            const loading = element.querySelector('.loading-overlay');
            if (loading) loading.remove();
        };
    }

    setupAccessibility() {
        // Skip to main content link
        const skipLink = document.createElement('a');
        skipLink.href = '#main-content';
        skipLink.className = 'skip-to-content';
        skipLink.textContent = 'Skip to main content';
        skipLink.style.cssText = `
            position: absolute;
            top: -40px;
            left: 0;
            background: #0d6efd;
            color: white;
            padding: 8px;
            z-index: 9999;
        `;
        skipLink.addEventListener('focus', () => {
            skipLink.style.top = '0';
        });
        skipLink.addEventListener('blur', () => {
            skipLink.style.top = '-40px';
        });
        document.body.insertBefore(skipLink, document.body.firstChild);

        // Add main content ID
        const mainContent = document.querySelector('main') || document.querySelector('.container');
        if (mainContent) {
            mainContent.id = 'main-content';
        }

        // ARIA labels for icons
        document.querySelectorAll('.bi').forEach(icon => {
            if (!icon.getAttribute('aria-label')) {
                const parentText = icon.parentElement.textContent.trim();
                if (parentText) {
                    icon.setAttribute('aria-label', parentText);
                }
            }
        });

        // Focus trap for modals
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                const openModal = document.querySelector('.modal.show');
                if (openModal) {
                    bootstrap.Modal.getInstance(openModal).hide();
                }
            }
        });
    }

    setupPerformance() {
        // Lazy loading images
        if ('IntersectionObserver' in window) {
            const imageObserver = new IntersectionObserver((entries) => {
                entries.forEach(entry => {
                    if (entry.isIntersecting) {
                        const img = entry.target;
                        img.src = img.dataset.src;
                        if (img.dataset.srcset) {
                            img.srcset = img.dataset.srcset;
                        }
                        imageObserver.unobserve(img);
                    }
                });
            });

            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        }

        // Debounce scroll events
        let scrollTimeout;
        window.addEventListener('scroll', () => {
            clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => {
                // Handle scroll-based operations
            }, 100);
        });
    }

    createToastContainer() {
        const container = document.createElement('div');
        container.id = 'toast-container';
        container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
        `;
        document.body.appendChild(container);
        return container;
    }

    getToastIcon(type) {
        const icons = {
            'success': 'bi-check-circle-fill',
            'danger': 'bi-exclamation-triangle-fill',
            'warning': 'bi-exclamation-circle-fill',
            'info': 'bi-info-circle-fill',
            'primary': 'bi-info-circle-fill'
        };
        return icons[type] || 'bi-info-circle-fill';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.edsgUsability = new EDSGUsability();

    // Add loading state to buttons on click
    document.addEventListener('click', (e) => {
        const button = e.target.closest('button[type="submit"], .btn[type="submit"]');
        if (button && !button.classList.contains('no-loading')) {
            const originalText = button.innerHTML;
            button.innerHTML = `
                <span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                Processando...
            `;
            button.disabled = true;

            // Reset after 5 seconds (fallback)
            setTimeout(() => {
                button.innerHTML = originalText;
                button.disabled = false;
            }, 5000);
        }
    });
});