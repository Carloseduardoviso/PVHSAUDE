(() => {
    const empresa = document.getElementById("CredenciadoId");
    const plano = document.getElementById("PlanoId");
    const catalogo = document.getElementById("planos-empresas");
    if (empresa && plano && catalogo) {
        const planos = JSON.parse(catalogo.textContent);
        const empresas = JSON.parse(document.getElementById("empresas-planos").textContent);
        const valor = document.getElementById("valor-plano");
        const aviso = document.getElementById("plano-aviso");
        const moeda = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });
        const periodos = ["Mensal", "Trimestral", "Semestral", "Anual"];
        function mostrarValor() {
            const item = planos.find(p => p.Id === plano.value);
            valor.value = item ? moeda.format(item.Valor) + " / " + periodos[item.Periodicidade] : "";
        }
        function filtrar(selecionado) {
            const empresaSelecionada = empresas.find(e => e.Id === empresa.value);
            const item = planos.find(p => p.Id === empresaSelecionada?.PlanoId);
            plano.value = item?.Id ?? "";
            document.getElementById("nome-plano").value = item?.Nome ?? "";
            aviso.textContent = !empresa.value ? "Selecione a empresa credenciada." :
                !item ? "Esta empresa ainda não possui plano. Vincule um plano no cadastro da empresa." :
                selecionado && selecionado !== item.Id ? "Ao salvar, será utilizado o plano atual da empresa." : "";
            mostrarValor();
        }
        empresa.addEventListener("change", () => filtrar(""));
        plano.addEventListener("change", mostrarValor);
        filtrar(plano.value);
    }
    if (window.jQuery?.validator) {
        window.jQuery.validator.addMethod("birthdate", function (value, element) {
            if (this.optional(element)) return true;
            const today = new Date();
            const max = today.getFullYear() + "-" + String(today.getMonth() + 1).padStart(2, "0") + "-" + String(today.getDate()).padStart(2, "0");
            return /^\d{4}-\d{2}-\d{2}$/.test(value) && value >= "1900-01-01" && value <= max && !Number.isNaN(Date.parse(value));
        }, "Informe uma data de nascimento válida, entre 01/01/1900 e hoje.");
    }
    function format(value, pattern) {
        let index = 0;
        let result = "";
        for (const char of pattern) {
            if (index >= value.length) break;
            result += char === "0" ? value[index++] : char;
        }
        return result;
    }
    function mask(input) {
        const type = input.dataset.mask;
        if (!type) return;
        const limit = type === "documento" ? 14 : 11;
        const digits = input.value.replace(/\D/g, "").slice(0, limit);
        const pattern = type === "telefone"
            ? (digits.length > 10 ? "(00) 00000-0000" : "(00) 0000-0000")
            : (type === "documento" && digits.length > 11 ? "00.000.000/0000-00" : "000.000.000-00");
        const start = input.selectionStart;
        const count = input.value.slice(0, start ?? input.value.length).replace(/\D/g, "").length;
        input.value = format(digits, pattern);
        if (document.activeElement === input && start !== null) {
            let position = 0, seen = 0;
            while (position < input.value.length && seen < count) {
                if (/\d/.test(input.value[position])) seen++;
                position++;
            }
            input.setSelectionRange(position, position);
        }
    }
    document.querySelectorAll("[data-mask]").forEach(mask);
    document.addEventListener("input", event => mask(event.target));
    const toggle = document.getElementById("incluir-dependentes");
    const fields = document.getElementById("dependentes-campos");
    const list = document.getElementById("dependentes-lista");
    if (!toggle || !fields || !list) return;
    const addButton = document.getElementById("adicionar-dependente");
    const limitMessage = document.getElementById("limite-dependentes");
    function updateLimit() {
        const reached = list.children.length >= 5;
        addButton.disabled = !toggle.checked || reached;
        if (limitMessage) limitMessage.hidden = !reached;
    }
    let next = list.children.length;
    function validate() {
        if (window.jQuery?.validator?.unobtrusive) {
            const form = window.jQuery(toggle.form);
            form.removeData("validator").removeData("unobtrusiveValidation");
            window.jQuery.validator.unobtrusive.parse(form);
        }
    }
    function add() {
        if (list.children.length >= 5) { updateLimit(); return; }
        const html = document.getElementById("dependente-template").innerHTML.replaceAll("__indice__", String(next++));
        list.insertAdjacentHTML("beforeend", html);
        const row = list.lastElementChild;
        row.querySelector('input[type="date"]').value = "";
        updateLimit();
        validate();
    }
    function update() {
        fields.hidden = !toggle.checked;
        toggle.setAttribute("aria-expanded", String(toggle.checked));
        if (toggle.checked && !list.children.length) add();
        fields.querySelectorAll("input, select, button").forEach(input => input.disabled = !toggle.checked);
        updateLimit();
    }
    toggle.addEventListener("change", update);
    document.getElementById("adicionar-dependente").addEventListener("click", add);
    list.addEventListener("click", event => {
        const button = event.target.closest(".remover-dependente");
        if (!button) return;
        button.closest(".dependente-item").remove();
        [...list.children].forEach((row, index) => {
            row.querySelectorAll("[name], [id], [for], [data-valmsg-for]").forEach(element => {
                for (const attribute of ["name", "id", "for", "data-valmsg-for"]) {
                    const value = element.getAttribute(attribute);
                    if (value) element.setAttribute(attribute, value.replace(/Dependentes\[\d+\]/g, "Dependentes[" + index + "]").replace(/Dependentes_\d+__/g, "Dependentes_" + index + "__"));
                }
            });
            row.querySelector('[name="Dependentes.Index"]').value = index;
        });
        next = list.children.length;
        if (!list.children.length) toggle.checked = false;
        update();
        validate();
    });
    update();
})();
