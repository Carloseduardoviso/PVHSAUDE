(() => {
    const periodicidade = document.getElementById("Periodicidade");
    const validade = document.getElementById("DataValidade");
    if (periodicidade && validade) {
        function preencherValidade() {
            // Valores do enum Periodicidade: Mensal=0, Trimestral=1, Semestral=2, Anual=3.
            const meses = { "0": 12, "1": 3, "2": 6, "3": 12 }[periodicidade.value];
            if (!meses) return;
            const hoje = new Date();
            const destino = new Date(hoje.getFullYear(), hoje.getMonth() + meses, 1);
            const ultimoDia = new Date(destino.getFullYear(), destino.getMonth() + 1, 0).getDate();
            destino.setDate(Math.min(hoje.getDate(), ultimoDia));
            validade.value = [destino.getFullYear(), String(destino.getMonth() + 1).padStart(2, "0"),
                String(destino.getDate()).padStart(2, "0")].join("-");
        }
        periodicidade.addEventListener("change", preencherValidade);
        if (!validade.value) preencherValidade();
    }

    function moeda(value) {
        const digits = value.replace(/\D/g, "").replace(/^0+(?=\d)/, "").slice(0, 18).padStart(3, "0");
        return "R$ " + digits.slice(0, -2).replace(/\B(?=(\d{3})+(?!\d))/g, ".") + "," + digits.slice(-2);
    }
    document.querySelectorAll('[data-mask="moeda"]').forEach(input => {
        // Decimal metadata emits numeric rules that do not accept Brazilian currency.
        input.removeAttribute("data-val-number");
        input.removeAttribute("data-val-range");
        input.removeAttribute("data-val-range-min");
        input.removeAttribute("data-val-range-max");
        input.value = moeda(input.value);
        input.addEventListener("input", () => { input.value = moeda(input.value); });
    });
    if (window.jQuery?.validator) {
        window.jQuery.validator.addMethod("moeda", value =>
            /^R\$ (?:\d{1,3}(?:\.\d{3})*),\d{2}$/.test(value),
            "Informe um valor válido, como R$ 150,00.");
        window.jQuery(() => {
            window.jQuery('[data-mask="moeda"]').each(function () {
                const field = window.jQuery(this);
                field.rules("remove", "number range");
                field.rules("add", { moeda: true });
            });
        });
    }
})();
