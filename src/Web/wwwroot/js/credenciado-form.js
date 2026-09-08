(() => {
    const imagem = document.getElementById("Imagem");
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
    const cidade = document.getElementById("Cidade");
    const uf = document.getElementById("Uf");
    const status = document.getElementById("cep-status");
    if (!cep || !cidade || !uf || !status) return;

    let pending;
    let revision = 0;
    cep.addEventListener("input", () => {
        revision++;
        pending?.abort();
        status.textContent = "";
        cidade.value = "";
        uf.value = "";
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
