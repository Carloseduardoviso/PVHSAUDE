(() => {
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
