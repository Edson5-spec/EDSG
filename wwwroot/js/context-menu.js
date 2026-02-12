// Context Menu Functionality
document.addEventListener('DOMContentLoaded', function () {
    // Initialize context menu
    initContextMenu();

    // Load unread message count
    loadUnreadCount();

    // Setup context menu button
    const contextButton = document.getElementById('contextMenuButton');
    if (contextButton) {
        contextButton.addEventListener('click', function (e) {
            updateContextMenu();
        });
    }

    // Mobile menu toggle
    const mobileMenuButtons = document.querySelectorAll('.mobile-context-trigger');
    mobileMenuButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            e.preventDefault();
            toggleMobileContextMenu();
        });
    });

    // Close context menu when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.context-menu') && !e.target.closest('#contextMenuButton')) {
            closeContextMenu();
        }
    });

    // Keyboard shortcuts
    document.addEventListener('keydown', function (e) {
        // Ctrl + M for context menu
        if (e.ctrlKey && e.key === 'm') {
            e.preventDefault();
            toggleContextMenu();
        }

        // Escape to close
        if (e.key === 'Escape') {
            closeContextMenu();
        }
    });

    // Context menu items hover effect
    const menuItems = document.querySelectorAll('.context-menu .dropdown-item');
    menuItems.forEach(item => {
        item.addEventListener('mouseenter', function () {
            this.style.transform = 'translateX(5px)';
            this.style.transition = 'transform 0.2s ease';
        });

        item.addEventListener('mouseleave', function () {
            this.style.transform = 'translateX(0)';
        });
    });
});

function initContextMenu() {
    // Add context menu to all pages
    const currentPath = window.location.pathname;
    const controller = getControllerFromPath(currentPath);
    const action = getActionFromPath(currentPath);

    // Update menu icon and title based on current page
    updateContextMenuIcon(controller, action);
    updateContextMenuTitle(controller, action);
}

function updateContextMenu() {
    const currentPath = window.location.pathname;
    const controller = getControllerFromPath(currentPath);
    const action = getActionFromPath(currentPath);

    // Update quick actions based on context
    updateQuickActions(controller, action);

    // Highlight current context
    highlightCurrentContext(controller, action);
}

function toggleContextMenu() {
    const menu = document.querySelector('.context-menu');
    const button = document.getElementById('contextMenuButton');

    if (menu && button) {
        const bsDropdown = bootstrap.Dropdown.getInstance(button);
        if (bsDropdown) {
            bsDropdown.toggle();
        } else {
            new bootstrap.Dropdown(button).toggle();
        }
    }
}

function closeContextMenu() {
    const button = document.getElementById('contextMenuButton');
    if (button) {
        const bsDropdown = bootstrap.Dropdown.getInstance(button);
        if (bsDropdown) {
            bsDropdown.hide();
        }
    }
}

function toggleMobileContextMenu() {
    const overlay = document.querySelector('.context-menu-overlay');
    const menu = document.querySelector('.mobile-context-menu');

    if (overlay && menu) {
        if (menu.classList.contains('show')) {
            menu.classList.remove('show');
            overlay.classList.remove('show');
        } else {
            menu.classList.add('show');
            overlay.classList.add('show');
        }
    }
}

function updateContextMenuIcon(controller, action) {
    const iconMap = {
        'Home/Index': 'bi-house-gear',
        'Home/Procurar': 'bi-search',
        'Dashboard/Index': 'bi-speedometer',
        'Dashboard/Mensagens': 'bi-chat-dots',
        'Dashboard/Favoritos': 'bi-heart',
        'Dashboard/Servicos': 'bi-briefcase',
        'Premium/Index': 'bi-star',
        'Premium/Benefits': 'bi-star',
        'Admin/Index': 'bi-shield-check',
        'Account/Profile': 'bi-person-gear'
    };

    const key = `${controller}/${action}`;
    const iconClass = iconMap[key] || 'bi-gear';

    const icon = document.querySelector('#contextMenuButton i');
    if (icon) {
        icon.className = `bi ${iconClass}`;
    }
}

function updateContextMenuTitle(controller, action) {
    const titleMap = {
        'Home/Index': 'Início',
        'Home/Procurar': 'Procurar',
        'Dashboard/Index': 'Dashboard',
        'Dashboard/Mensagens': 'Mensagens',
        'Dashboard/Favoritos': 'Favoritos',
        'Dashboard/Servicos': 'Serviços',
        'Premium/Index': 'Premium',
        'Premium/Benefits': 'Premium',
        'Admin/Index': 'Admin',
        'Account/Profile': 'Perfil'
    };

    const key = `${controller}/${action}`;
    const title = titleMap[key] || 'Menu';

    const titleElement = document.querySelector('.context-menu-label');
    if (titleElement) {
        titleElement.textContent = title;
    }
}

function updateQuickActions(controller, action) {
    // This would be populated via AJAX in a real implementation
    console.log(`Updating quick actions for ${controller}/${action}`);

    // Example: Update unread count badge
    updateUnreadBadge();
}

function highlightCurrentContext(controller, action) {
    // Remove all active highlights
    const menuItems = document.querySelectorAll('.context-menu .dropdown-item');
    menuItems.forEach(item => {
        item.classList.remove('active');
        item.querySelector('.badge')?.remove();
    });

    // Add highlight to current context
    const currentItem = findMenuItemForContext(controller, action);
    if (currentItem) {
        currentItem.classList.add('active');

        // Add current badge
        const badge = document.createElement('span');
        badge.className = 'badge bg-primary float-end';
        badge.textContent = 'Aqui';
        currentItem.appendChild(badge);
    }
}

function findMenuItemForContext(controller, action) {
    const hrefMap = {
        'Home/Index': '/',
        'Home/Procurar': '/Home/Procurar',
        'Dashboard/Index': '/Dashboard',
        'Dashboard/Mensagens': '/Dashboard/Mensagens',
        'Dashboard/Favoritos': '/Dashboard/Favoritos',
        'Dashboard/Servicos': '/Dashboard/Servicos',
        'Premium/Index': '/Premium',
        'Premium/Benefits': '/Premium/Benefits',
        'Admin/Index': '/Admin',
        'Account/Profile': '/Account/Profile'
    };

    const key = `${controller}/${action}`;
    const href = hrefMap[key];

    if (href) {
        return document.querySelector(`.context-menu a[href="${href}"]`);
    }

    return null;
}

function getControllerFromPath(path) {
    const parts = path.split('/').filter(p => p);
    return parts.length > 0 ? parts[0] : 'Home';
}

function getActionFromPath(path) {
    const parts = path.split('/').filter(p => p);
    return parts.length > 1 ? parts[1] : 'Index';
}

async function loadUnreadCount() {
    try {
        // In a real application, this would be an API call
        // const response = await fetch('/api/messages/unread-count');
        // const data = await response.json();

        // Simulated data
        const unreadCount = Math.floor(Math.random() * 5); // Random 0-4

        updateUnreadBadge(unreadCount);
    } catch (error) {
        console.error('Error loading unread count:', error);
    }
}

function updateUnreadBadge(count = 0) {
    const badges = document.querySelectorAll('#unreadCount, #mobileUnreadCount');

    badges.forEach(badge => {
        if (count > 0) {
            badge.textContent = count;
            badge.style.display = 'inline-block';

            // Add pulse animation for new messages
            if (count > parseInt(badge.textContent || 0)) {
                badge.classList.add('pulse-animation');
                setTimeout(() => {
                    badge.classList.remove('pulse-animation');
                }, 1000);
            }
        } else {
            badge.style.display = 'none';
        }
    });
}

// Context menu animations
function animateContextMenu() {
    const menu = document.querySelector('.context-menu');
    if (menu) {
        menu.style.opacity = '0';
        menu.style.transform = 'translateY(-10px)';

        setTimeout(() => {
            menu.style.transition = 'opacity 0.3s ease, transform 0.3s ease';
            menu.style.opacity = '1';
            menu.style.transform = 'translateY(0)';
        }, 10);
    }
}

// Share page functionality
function sharePage() {
    if (navigator.share) {
        navigator.share({
            title: document.title,
            text: 'Confira esta página no EDSG',
            url: window.location.href
        });
    } else {
        // Fallback for browsers that don't support Web Share API
        navigator.clipboard.writeText(window.location.href).then(() => {
            alert('Link copiado para a área de transferência!');
        });
    }
}

// Initialize when page loads
window.addEventListener('load', function () {
    // Add animation to context menu when it opens
    const contextButton = document.getElementById('contextMenuButton');
    if (contextButton) {
        contextButton.addEventListener('shown.bs.dropdown', function () {
            animateContextMenu();
        });
    }

    // Check for new notifications every 30 seconds
    setInterval(loadUnreadCount, 30000);
});