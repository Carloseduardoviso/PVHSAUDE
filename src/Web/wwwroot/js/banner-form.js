const bannerInput = document.getElementById('Imagem');
bannerInput?.addEventListener('change', () => {
    bannerInput.setCustomValidity('');
    const file = bannerInput.files[0];
    if (!file) return;
    const url = URL.createObjectURL(file);
    const image = new Image();
    const finish = (message) => {
        URL.revokeObjectURL(url);
        if (bannerInput.files[0] !== file) return;
        bannerInput.setCustomValidity(message);
        if (message) bannerInput.reportValidity();
    };
    image.onload = () => finish(image.naturalWidth * 5 === image.naturalHeight * 16
        ? '' : 'Selecione uma imagem em paisagem na proporção 16:5, por exemplo 1600 × 500 pixels.');
    image.onerror = () => finish('Não foi possível ler a imagem. Envie um JPG, PNG ou WEBP válido.');
    image.src = url;
});
