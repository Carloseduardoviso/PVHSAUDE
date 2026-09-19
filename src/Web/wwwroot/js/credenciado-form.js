(() => {
    const imagem = document.getElementById("Imagem");
    const imagens = document.getElementById("Imagens");
    const imagensSelecionadas = document.getElementById("imagens-selecionadas");
    const imagensPreview = document.getElementById("imagem-preview-container");
    const imagensSalvas = document.getElementById("imagem-salva-list");
    const imagensPreviewList = document.getElementById("imagem-preview-list");
    let arquivosSelecionados = [];
    let urlsPreview = [];
    const tiposImagem = ["image/jpeg", "image/png", "image/webp"];
    const renderizarImagens = () => {
        urlsPreview.forEach(url => URL.revokeObjectURL(url));
        urlsPreview = [];
        imagensPreviewList?.replaceChildren();
        imagensSelecionadas.textContent = arquivosSelecionados.length === 0
            ? "Selecione várias imagens JPG, PNG ou WEBP, até 5 MB cada."
            : `${arquivosSelecionados.length} imagem(ns) selecionada(s). JPG, PNG ou WEBP, até 5 MB cada.`;
        for (const [indice, arquivo] of arquivosSelecionados.entries()) {
            const url = URL.createObjectURL(arquivo);
            urlsPreview.push(url);
            const item = document.createElement("div");
            item.className = "d-flex flex-column align-items-center gap-1";
            const img = document.createElement("img");
            img.src = url;
            img.alt = arquivo.name;
            img.style.cssText = "width:96px;height:72px;object-fit:cover;border-radius:6px;border:2px solid #198754;padding:2px";
            const remover = document.createElement("button");
            remover.type = "button";
            remover.className = "btn btn-sm btn-outline-danger";
            remover.textContent = "Remover";
            remover.addEventListener("click", () => {
                arquivosSelecionados.splice(indice, 1);
                const transfer = new DataTransfer();
                arquivosSelecionados.forEach(arquivoAtual => transfer.items.add(arquivoAtual));
                imagens.files = transfer.files;
                renderizarImagens();
            });
            item.append(img, remover);
            imagensPreviewList?.appendChild(item);
        }
        if (imagensPreview) imagensPreview.hidden = arquivosSelecionados.length === 0 && (!imagensSalvas || imagensSalvas.children.length === 0);
    };
    if (imagens && imagensSelecionadas) imagens.addEventListener("change", () => {
        const novosArquivos = [...(imagens.files || [])].filter(arquivo => tiposImagem.includes(arquivo.type) && arquivo.size <= 5 * 1024 * 1024);
        const existentes = new Set(arquivosSelecionados.map(arquivo => `${arquivo.name}|${arquivo.size}|${arquivo.lastModified}`));
        arquivosSelecionados.push(...novosArquivos.filter(arquivo => !existentes.has(`${arquivo.name}|${arquivo.size}|${arquivo.lastModified}`)));
        const transfer = new DataTransfer();
        arquivosSelecionados.forEach(arquivo => transfer.items.add(arquivo));
        imagens.files = transfer.files;
        renderizarImagens();
    });
    const preview = document.getElementById("imagem-preview");
    const previewContainer = document.getElementById("imagem-preview-container");
    let imagemUrl;
    if (imagem && preview && previewContainer) {
        imagem.addEventListener("change", () => {
            const arquivo = imagem.files?.[0];
            if (!arquivo) {
                preview.removeAttribute("src");
                previewContainer.hidden = true;
                if (imagemUrl) URL.revokeObjectURL(imagemUrl);
                imagemUrl = null;
                return;
            }
            if (!["image/jpeg", "image/png", "image/webp"].includes(arquivo.type) || arquivo.size > 5 * 1024 * 1024) {
                preview.removeAttribute("src");
                previewContainer.hidden = true;
                return;
            }
            if (imagemUrl) URL.revokeObjectURL(imagemUrl);
            imagemUrl = URL.createObjectURL(arquivo);
            preview.src = imagemUrl;
            preview.style.width = "140px";
            preview.style.height = "140px";
            preview.style.objectFit = "cover";
            preview.style.borderRadius = "50%";
            preview.style.border = "3px solid #198754";
            preview.style.padding = "3px";
            previewContainer.hidden = false;
        });
    }

    function mask(input) {
        let digits = input.value.replace(/\D/g, "");
        const kind = input.dataset.credMask;
        digits = digits.slice(0, kind === "cnpj" ? 14 : kind === "cep" ? 8 : 11);
        const pattern = kind === "cnpj" ? "00.000.000/0000-00" : kind === "cep" ? "00000-000" : digits.length > 10 ? "(00) 00000-0000" : "(00) 0000-0000";
        let index = 0, formatted = "";
        for (const char of pattern) {
            if (index >= digits.length) break;
            formatted += char === "0" ? digits[index++] : char;
        }
        input.value = formatted;
    }
    document.querySelectorAll("[data-cred-mask]").forEach(input => {
        mask(input);
        input.addEventListener("input", () => mask(input));
    });
    const cep = document.getElementById("Cep");
    const numero = document.getElementById("Numero");
    const endereco = document.getElementById("Endereco");
    const cidade = document.getElementById("Cidade");
    const uf = document.getElementById("Uf");
    const status = document.getElementById("cep-status");
    if (!cep || !numero || !endereco || !cidade || !uf || !status) return;

    let pending;
    let revision = 0;
    // Existing addresses on edit forms are user data and must not be replaced by CEP lookup.
    let addressWasEdited = Boolean(endereco.value.trim());
    let addressParts;
    const composeAddress = () => {
        if (!addressParts || addressWasEdited) return;
        const locality = cidade.value || addressParts.localidade;
        const state = uf.value || addressParts.uf;
        if (!addressParts.logradouro || !locality || !state) return;
        endereco.value = `${addressParts.logradouro}${numero.value.trim() ? ", " + numero.value.trim() : ""} - ${addressParts.bairro || ""} - ${locality}/${state}`;
    };
    endereco.addEventListener("input", () => { addressWasEdited = true; });
    numero.addEventListener("input", composeAddress);
    cep.addEventListener("input", () => {
        revision++;
        pending?.abort();
        status.textContent = "";
        addressParts = null;
    });
    cep.addEventListener("blur", async () => {
        const digits = cep.value.replace(/\D/g, "");
        pending?.abort();
        const current = ++revision;
        if (!digits) { status.textContent = ""; return; }
        if (digits.length !== 8) {
            status.textContent = "Informe um CEP com 8 dígitos.";
            return;
        }
        const controller = new AbortController();
        pending = controller;
        const timeout = window.setTimeout(() => controller.abort(), 10000);
        const initialCity = cidade.value;
        const initialUf = uf.value;
        const initialEndereco = endereco.value;
        status.textContent = "Consultando CEP...";
        try {
            const response = await fetch("https://viacep.com.br/ws/" + digits + "/json/", { signal: controller.signal });
            if (!response.ok) throw new Error("Consulta indisponível");
            const data = await response.json();
            if (current !== revision) return;
            if (data.erro || !data.localidade || !data.uf) {
                status.textContent = "CEP não encontrado. Confira o CEP ou preencha cidade e UF manualmente.";
                return;
            }
            // Keep manual edits made while the request was in progress.
            if (cidade.value === initialCity) cidade.value = data.localidade;
            if (uf.value === initialUf) uf.value = data.uf;
            addressParts = data;
            if (endereco.value === initialEndereco) composeAddress();
            cidade.dispatchEvent(new Event("change", { bubbles: true }));
            uf.dispatchEvent(new Event("change", { bubbles: true }));
            status.textContent = "Consulta de CEP concluída.";
        } catch {
            if (current === revision)
                status.textContent = "Não foi possível consultar o CEP. Preencha cidade e UF manualmente.";
        } finally {
            window.clearTimeout(timeout);
            if (current === revision) pending = null;
        }
    });
})();
