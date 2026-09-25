(() => {
    "use strict";

    function formatarCpf(valor) {
        const digitos = valor.replace(/\D/g, "").slice(0, 11);
        return digitos
            .replace(/^(\d{3})(\d)/, "$1.$2")
            .replace(/^(\d{3})\.(\d{3})(\d)/, "$1.$2.$3")
            .replace(/^(\d{3})\.(\d{3})\.(\d{3})(\d)/, "$1.$2.$3-$4");
    }

    document.querySelectorAll("[data-cpf-mask]").forEach(input => {
        input.value = formatarCpf(input.value);
        input.addEventListener("input", () => {
            const antes = input.value.slice(0, input.selectionStart ?? input.value.length).replace(/\D/g, "").length;
            input.value = formatarCpf(input.value);
            if (document.activeElement === input) {
                let posicao = 0;
                let digitos = 0;
                while (posicao < input.value.length && digitos < antes) {
                    if (/\d/.test(input.value[posicao])) digitos++;
                    posicao++;
                }
                input.setSelectionRange(posicao, posicao);
            }
        });
    });

    document.querySelectorAll("[data-password-toggle]").forEach(botao => {
        botao.addEventListener("click", () => {
            const campo = document.getElementById(botao.getAttribute("aria-controls"));
            if (!campo) return;
            const visivel = campo.type === "password";
            campo.type = visivel ? "text" : "password";
            botao.setAttribute("aria-pressed", String(visivel));
            botao.setAttribute("aria-label", visivel ? "Ocultar senha" : "Mostrar senha");
            botao.classList.toggle("is-visible", visivel);
            campo.focus({ preventScroll: true });
        });
    });
})();
