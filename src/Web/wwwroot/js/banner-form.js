const bannerInput = document.getElementById('Imagem');
const posicaoInput = document.getElementById('posicao-banner');
const ajuda = document.getElementById('imagem-ajuda');
const regras = {
    Central: { largura: 16, altura: 5, texto: 'Obrigatório: formato paisagem na proporção 16:5. Tamanho recomendado: 1600 × 500 pixels.' },
    LateralDireita: { largura: 4, altura: 5, texto: 'Obrigatório: formato retrato na proporção 4:5. Tamanho recomendado: 400 × 500 pixels.' },
    LateralEsquerda: { largura: 4, altura: 5, texto: 'Obrigatório: formato retrato na proporção 4:5. Tamanho recomendado: 400 × 500 pixels.' },
    InferiorDireita: { largura: 16, altura: 5, texto: 'Obrigatório: formato paisagem na proporção 16:5. Tamanho recomendado: 800 × 250 pixels.' },
    InferiorEsquerda: { largura: 16, altura: 5, texto: 'Obrigatório: formato paisagem na proporção 16:5. Tamanho recomendado: 800 × 250 pixels.' }
};
function regraAtual() { return regras[posicaoInput?.value] ?? regras.Central; }
function atualizarAjuda() { if (ajuda) ajuda.textContent = `JPG, PNG ou WEBP, até 5 MB. ${regraAtual().texto} ${bannerInput?.dataset.novo === 'true' ? '' : 'Deixe em branco para manter a imagem atual.'}`; }
function validarImagem() {
    if (!bannerInput) return;
    bannerInput.setCustomValidity('');
    const file = bannerInput.files[0];
    if (!file) return;
    const url = URL.createObjectURL(file); const image = new Image(); const regra = regraAtual();
    const finalizar = mensagem => { URL.revokeObjectURL(url); if (bannerInput.files[0] !== file) return; bannerInput.setCustomValidity(mensagem); if (mensagem) bannerInput.reportValidity(); };
    image.onload = () => finalizar(image.naturalWidth * regra.altura === image.naturalHeight * regra.largura ? '' : `Selecione uma imagem na proporção ${regra.largura}:${regra.altura}.`);
    image.onerror = () => finalizar('Não foi possível ler a imagem. Envie um JPG, PNG ou WEBP válido.'); image.src = url;
}
posicaoInput?.addEventListener('change', () => { atualizarAjuda(); validarImagem(); });
bannerInput?.addEventListener('change', validarImagem);
atualizarAjuda();