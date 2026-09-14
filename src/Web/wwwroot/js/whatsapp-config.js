(() => {
    'use strict';
    const input = document.getElementById('Telefone');
    const value = document.getElementById('TelefoneValor');
    if (!input || !value) return;

    function update() {
        const caret = input.selectionStart;
        let before = input.value.slice(0, caret ?? input.value.length).replace(/\D/g, '').length;
        let phone = input.value.replace(/\D/g, '');
        // Existing records include Brazil's country code; the field displays only DDD and number.
        if (phone.startsWith('55') && [12, 13].includes(phone.length)) {
            phone = phone.slice(2);
            before = Math.max(0, before - 2);
        }
        phone = phone.slice(0, 11);
        value.value = phone ? '55' + phone : '';
        const area = phone.slice(0, 2);
        const local = phone.slice(2);
        const split = local.length > 8 ? 5 : 4;
        const formatted = (area ? '(' + area : '') + (local ? ') ' : '')
            + local.slice(0, split) + (local.length > split ? '-' + local.slice(split) : '');
        input.value = formatted;
        input.setCustomValidity(!phone || /^[1-9][0-9]{9,10}$/.test(phone)
            ? '' : 'Informe um telefone válido com DDD.');
        if (document.activeElement === input && caret !== null) {
            let position = 0;
            let count = 0;
            while (position < formatted.length && count < before) {
                if (/\d/.test(formatted[position])) count++;
                position++;
            }
            input.setSelectionRange(position, position);
        }
    }
    input.addEventListener('input', update);
    input.addEventListener('change', update);
    input.form.addEventListener('submit', event => {
        update();
        if (!input.reportValidity()) event.preventDefault();
    });
    update();
})();
