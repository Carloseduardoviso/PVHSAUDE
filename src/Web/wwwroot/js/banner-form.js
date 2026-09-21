const bannerInput = document.getElementById('Imagem');
const posicaoInput = document.getElementById('posicao-banner');
const ajuda = document.getElementById('imagem-ajuda');
const tituloInput = document.getElementById('banner-titulo-input');
const ativoInput = document.getElementById('banner-ativo-input');
const bannerPreviewTitulo = document.getElementById('banner-preview-titulo');
const bannerPreviewContainer = document.getElementById('banner-preview-container');
const bannerPreviewFrame = document.getElementById('banner-preview-frame');
const bannerPreviewImg = document.getElementById('banner-preview-img');
const bannerPreviewPlaceholder = document.getElementById('banner-preview-placeholder');
const bannerPosicaoBadge = document.getElementById('banner-posicao-badge');
const bannerAtivoBadge = document.getElementById('banner-ativo-badge');
const bannerPreviewAreaDesc = document.getElementById('banner-preview-area-desc');
const bannerDimensoesInfo = document.getElementById('banner-dimensoes-info');

const regras = {
    Central: { largura: 16, altura: 5, texto: 'Obrigatório: formato paisagem na proporção 16:5. Tamanho recomendado: 1920 × 600 pixels.', desc: 'Área central de destaques (formato paisagem)', uso: 'campanha ou oferta principal.' },
    LateralDireita: { largura: 4, altura: 5, texto: 'Obrigatório: formato retrato na proporção 4:5. Tamanho recomendado: 800 × 1000 pixels.', desc: 'Coluna lateral direita (formato retrato)', uso: 'serviço, categoria ou benefício complementar.' },
    LateralEsquerda: { largura: 4, altura: 5, texto: 'Obrigatório: formato retrato na proporção 4:5. Tamanho recomendado: 800 × 1000 pixels.', desc: 'Coluna lateral esquerda (formato retrato)', uso: 'serviço, categoria ou benefício complementar.' },
    InferiorDireita: { largura: 16, altura: 5, texto: 'Obrigatório: formato paisagem na proporção 16:5. Tamanho recomendado: 960 × 300 pixels.', desc: 'Coluna inferior direita (formato paisagem)', uso: 'campanha secundária diferente da oferta principal.' },
    InferiorEsquerda: { largura: 16, altura: 5, texto: 'Obrigatório: formato paisagem na proporção 16:5. Tamanho recomendado: 960 × 300 pixels.', desc: 'Coluna inferior esquerda (formato paisagem)', uso: 'campanha secundária diferente da oferta principal.' }
};

let objectUrl = null;

function regraAtual() { return regras[posicaoInput?.value] ?? regras.Central; }

function atualizarAjuda() {
    if (ajuda) ajuda.textContent = `JPG, PNG ou WEBP, até 5 MB. ${regraAtual().texto} Uso recomendado: ${regraAtual().uso} ${bannerInput?.dataset.novo === 'true' ? '' : 'Deixe em branco para manter a imagem atual.'}`;
}

function atualizarPosicaoVisual() {
    const regra = regraAtual();
    const posTexto = posicaoInput?.selectedOptions[0]?.text || posicaoInput?.value || 'Central';
    const ehRetrato = regra.largura === 4 && regra.altura === 5;

    if (bannerPreviewFrame) {
        bannerPreviewFrame.style.aspectRatio = `${regra.largura} / ${regra.altura}`;
    }
    if (bannerPreviewContainer) {
        bannerPreviewContainer.style.maxWidth = ehRetrato ? '280px' : '100%';
    }
    if (bannerPosicaoBadge) {
        bannerPosicaoBadge.textContent = `${posTexto} (${regra.largura}:${regra.altura})`;
    }
    if (bannerPreviewAreaDesc) {
        bannerPreviewAreaDesc.textContent = regra.desc;
    }
}

function validarImagem() {
    if (!bannerInput) return;
    bannerInput.setCustomValidity('');
    const file = bannerInput.files[0];
    if (!file) {
        if (bannerDimensoesInfo) bannerDimensoesInfo.textContent = '';
        return;
    }

    if (objectUrl) {
        URL.revokeObjectURL(objectUrl);
        objectUrl = null;
    }

    const regra = regraAtual();
    const finalizar = (mensagem, w, h) => {
        if (bannerInput.files[0] !== file) return;
        bannerInput.setCustomValidity(mensagem);

        if (mensagem) {
            if (bannerDimensoesInfo && w && h) {
                bannerDimensoesInfo.textContent = `${w} × ${h} px (fora da proporção ${regra.largura}:${regra.altura})`;
            }
            bannerInput.reportValidity();
        } else {
            if (bannerPreviewImg && objectUrl) {
                bannerPreviewImg.src = objectUrl;
                bannerPreviewImg.classList.remove('d-none');
            }
            if (bannerPreviewPlaceholder) {
                bannerPreviewPlaceholder.classList.add('d-none');
            }
            if (bannerDimensoesInfo && w && h) {
                bannerDimensoesInfo.textContent = `${w} × ${h} px (${regra.largura}:${regra.altura})`;
            }
        }
    };

    if (file.size <= 0 || file.size > 5 * 1024 * 1024) {
        finalizar('A imagem deve ter até 5 MB.');
        return;
    }

    objectUrl = URL.createObjectURL(file);
    const image = new Image();
    image.onload = () => {
        const w = image.naturalWidth;
        const h = image.naturalHeight;
        const proporcaoValida = (w * regra.altura === h * regra.largura);
        finalizar(proporcaoValida ? '' : `Selecione uma imagem na proporção ${regra.largura}:${regra.altura}.`, w, h);
    };
    image.onerror = () => finalizar('Não foi possível ler a imagem. Envie um JPG, PNG ou WEBP válido.');
    image.src = objectUrl;
}

tituloInput?.addEventListener('input', () => {
    if (bannerPreviewTitulo) {
        bannerPreviewTitulo.textContent = tituloInput.value.trim() || 'Título do banner';
    }
});

ativoInput?.addEventListener('change', () => {
    if (bannerAtivoBadge) {
        bannerAtivoBadge.textContent = ativoInput.checked ? 'Ativo' : 'Inativo';
        bannerAtivoBadge.className = 'badge ' + (ativoInput.checked ? 'bg-success' : 'bg-secondary');
    }
});

posicaoInput?.addEventListener('change', () => {
    atualizarAjuda();
    atualizarPosicaoVisual();
    validarImagem();
});

bannerInput?.addEventListener('change', validarImagem);

atualizarAjuda();
atualizarPosicaoVisual();
