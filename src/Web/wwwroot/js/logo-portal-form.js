const logoInput = document.getElementById('logo');
const erroSpan = document.getElementById('logo-erro');
const logoPreviewImg = document.getElementById('logo-preview-img');
const logoPreviewPlaceholder = document.getElementById('logo-preview-placeholder');
const logoStatusBadge = document.getElementById('logo-status-badge');
const logoDimensoesInfo = document.getElementById('logo-dimensoes-info');
let objectUrl = null;

function validarLogo() {
    if (!logoInput) return;
    logoInput.setCustomValidity('');
    if (erroSpan) erroSpan.textContent = '';
    const file = logoInput.files[0];
    if (!file) return;

    if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
        objectUrl = null;
    }

    const finalizar = (mensagem, w, h) => {
        if (logoInput.files[0] !== file) return;
        logoInput.setCustomValidity(mensagem);
        if (erroSpan) erroSpan.textContent = mensagem;

        if (mensagem) {
            if (logoStatusBadge) {
                logoStatusBadge.textContent = 'Imagem fora do padrão';
                logoStatusBadge.className = 'badge bg-danger';
            }
            if (logoDimensoesInfo && w && h) {
                logoDimensoesInfo.textContent = `${w} × ${h} px (${(w / h).toFixed(2)}:1) — fora do padrão`;
            }
            logoInput.reportValidity();
        } else {
            if (logoPreviewImg && objectUrl) {
                logoPreviewImg.src = objectUrl;
                logoPreviewImg.classList.remove('d-none');
            }
            if (logoPreviewPlaceholder) {
                logoPreviewPlaceholder.classList.add('d-none');
            }
            if (logoStatusBadge) {
                logoStatusBadge.textContent = 'Nova logo (pré-visualização)';
                logoStatusBadge.className = 'badge bg-success';
            }
            if (logoDimensoesInfo && w && h) {
                logoDimensoesInfo.textContent = `${w} × ${h} px (${(w / h).toFixed(2)}:1)`;
            }
        }
    };

    if (file.size <= 0 || file.size > 5 * 1024 * 1024) {
        finalizar('A logo deve ter até 5 MB.');
        return;
    }

    const tiposValidos = ['image/jpeg', 'image/png', 'image/webp'];
    const extensaoValida = /\.(jpe?g|png|webp)$/i.test(file.name);
    if (!tiposValidos.includes(file.type) && !extensaoValida) {
        finalizar('Envie uma logo JPG, PNG ou WEBP válida.');
        return;
    }

    objectUrl = URL.createObjectURL(file);
    const image = new Image();
    image.onload = () => {
        const w = image.naturalWidth;
        const h = image.naturalHeight;
        const proporcaoValida = (w * 1 === h * 6) || (h > 0 && Math.abs((w / h) - 6.0) <= 0.02);
        finalizar(proporcaoValida ? '' : 'A logo deve estar na proporção 6:1 (recomendado: 1920 × 320 pixels).', w, h);
    };
    image.onerror = () => finalizar('Não foi possível ler a imagem. Envie um JPG, PNG ou WEBP válido.');
    image.src = objectUrl;
}

logoInput?.addEventListener('change', validarLogo);

const logoForm = logoInput?.closest('form');
logoForm?.addEventListener('submit', e => {
    if (logoInput && !logoInput.checkValidity()) {
        e.preventDefault();
        logoInput.reportValidity();
    }
});
