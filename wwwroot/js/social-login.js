// social-login.js
function loginWithGoogle(context) {
    console.log('Iniciando login com Google para:', context);

    // Encontrar todos os botões Google
    const googleButtons = document.querySelectorAll('button[onclick*="loginWithGoogle"]');

    // Desativar todos os botões Google
    googleButtons.forEach(button => {
        const originalText = button.innerHTML;
        button.disabled = true;
        button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> A redirecionar para Google...';

        // Restaurar após 5 segundos (fallback)
        setTimeout(() => {
            button.disabled = false;
            button.innerHTML = originalText;
        }, 5000);
    });

    // Buscar o token anti-forgery
    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenElement ? tokenElement.value : '';

    // Criar formulário dinâmico
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Account/ExternalLogin';
    form.style.display = 'none';

    // Adicionar inputs
    const providerInput = document.createElement('input');
    providerInput.type = 'hidden';
    providerInput.name = 'provider';
    providerInput.value = 'Google';

    const tokenInput = document.createElement('input');
    tokenInput.type = 'hidden';
    tokenInput.name = '__RequestVerificationToken';
    tokenInput.value = token;

    // Construir formulário
    form.appendChild(providerInput);
    form.appendChild(tokenInput);
    document.body.appendChild(form);

    console.log('Submetendo formulário para Google OAuth...');
    form.submit();
}

// Inicializar quando a página carregar
document.addEventListener('DOMContentLoaded', function () {
    console.log('Social login scripts carregados');

    // Log para debug
    const googleButtons = document.querySelectorAll('button[onclick*="loginWithGoogle"]');
    console.log(`Botões Google encontrados: ${googleButtons.length}`);
});