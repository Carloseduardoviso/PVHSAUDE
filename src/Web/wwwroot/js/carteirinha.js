(() => {
    'use strict';
    const form = document.getElementById('carteirinha-form');
    if (!form) return;
    const steps = [...form.querySelectorAll('[data-step]')];
    const indicators = [...document.querySelectorAll('.carteirinha-steps li')];
    const next = document.getElementById('step-next');
    const back = document.getElementById('step-back');
    const review = document.getElementById('carteirinha-review');
    const modal = document.getElementById('esportes-modal');
    const teamSelect = document.getElementById('sports-team');
    let current = 0;
    let selectedTeam = '';
    const field = name => form.elements.namedItem(name);
    const digits = value => value.replace(/\D/g, '');
    function validCpf(value) {
        const cpf = digits(value);
        if (!/^\d{11}$/.test(cpf) || /^(\d)\1{10}$/.test(cpf)) return false;
        for (let length = 9; length <= 10; length++) {
            let sum = 0;
            for (let i = 0; i < length; i++) sum += Number(cpf[i]) * (length + 1 - i);
            const check = (sum * 10) % 11 % 10;
            if (check !== Number(cpf[length])) return false;
        }
        return true;
    }
    function validate(section) {
        for (const input of section.querySelectorAll('input, select')) {
            input.setCustomValidity('');
            if (input.required && input.type === 'text' && !input.value.trim()) input.setCustomValidity('Preencha este campo.');
            if (input.name === 'cpf' && !validCpf(input.value)) input.setCustomValidity('Informe um CPF válido.');
            if (input.name === 'telefone' && !/^\d{10,11}$/.test(digits(input.value))) input.setCustomValidity('Informe o telefone com DDD.');
            if (input.name === 'cep' && digits(input.value).length !== 8) input.setCustomValidity('Informe um CEP com 8 dígitos.');
            if (!input.reportValidity()) return false;
        }
        return true;
    }
    function showStep(index) {
        current = index;
        form.hidden = false;
        review.hidden = true;
        steps.forEach((section, i) => { section.hidden = i !== current; });
        indicators.forEach((indicator, i) => {
            indicator.classList.toggle('is-complete', i < current);
            indicator.querySelector('span').textContent = i < current ? '✓' : String(i + 1);
            if (i === current) indicator.setAttribute('aria-current', 'step');
            else indicator.removeAttribute('aria-current');
        });
        back.hidden = current === 0;
        next.textContent = current === 3 ? 'Revisar solicitação' : 'Próximo passo →';
        steps[current].querySelector('h2').focus();
    }
    function showReview() {
        const values = [
            ['Nome', field('nome').value.trim()], ['E-mail', field('email').value.trim()],
            ['WhatsApp', field('telefone').value], ['CPF', field('cpf').value],
            ['Endereço', `${field('rua').value}, ${field('numero').value} — ${field('bairro').value}, ${field('cidade').value}/${field('uf').value}, CEP ${field('cep').value}`],
            ['Complemento', field('complemento').value || 'Não informado'],
            ['Esportes', selectedTeam || 'Não incluído'], ['Pagamento', field('pagamento').value],
            ['Comunicações', field('comunicacoes').checked ? 'Desejo receber' : 'Não desejo receber']
        ];
        const list = document.getElementById('review-values');
        list.replaceChildren();
        values.forEach(([label, value]) => {
            const dt = document.createElement('dt');
            const dd = document.createElement('dd');
            dt.textContent = label;
            dd.textContent = value;
            list.append(dt, dd);
        });
        form.hidden = true;
        review.hidden = false;
        document.getElementById('review-title').focus();
    }
    next.addEventListener('click', () => {
        if (!validate(steps[current])) return;
        if (current < 3) showStep(current + 1);
        else showReview();
    });
    back.addEventListener('click', () => showStep(current - 1));
    document.getElementById('review-back').addEventListener('click', () => showStep(3));
    // Keep personal data in memory; this preview never posts or stores card details.
    form.addEventListener('submit', event => event.preventDefault());
    form.addEventListener('keydown', event => {
        if (event.key === 'Enter' && event.target.tagName === 'INPUT' && !['radio', 'checkbox'].includes(event.target.type)) {
            event.preventDefault();
            next.click();
        }
    });
    form.addEventListener('input', event => {
        if (event.target.setCustomValidity) event.target.setCustomValidity('');
        document.getElementById('preview-name').textContent = field('nome').value.trim() || 'Sua carteirinha';
    });
    field('cpf').addEventListener('input', event => {
        event.target.value = digits(event.target.value).slice(0, 11).replace(/^(\d{3})(\d)/, '$1.$2').replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3').replace(/(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4');
    });
    field('cep').addEventListener('input', event => { event.target.value = digits(event.target.value).slice(0, 8).replace(/^(\d{5})(\d)/, '$1-$2'); });
    form.querySelectorAll('[name="pagamento"]').forEach(input => input.addEventListener('change', () => {
        document.getElementById('summary-payment').textContent = input.value;
        document.getElementById('payment-note').textContent = input.value === 'Pix'
            ? 'Pix selecionado. O código de pagamento será disponibilizado quando a cobrança online estiver habilitada.'
            : `${input.value} selecionado. Os dados do cartão serão solicitados no ambiente de pagamento quando a cobrança estiver habilitada.`;
    }));
    function updateSports() {
        document.getElementById('sports-selection').textContent = selectedTeam ? `Time escolhido: ${selectedTeam}` : 'Nenhum time selecionado.';
        document.getElementById('summary-sport').textContent = selectedTeam || 'Sem esportes';
        document.getElementById('sports-open').textContent = selectedTeam ? 'Alterar time' : 'Incluir esportes';
        document.getElementById('sports-remove').hidden = !selectedTeam;
    }
    modal.addEventListener('show.bs.modal', () => { teamSelect.value = selectedTeam; });
    modal.addEventListener('shown.bs.modal', () => teamSelect.focus());
    document.getElementById('sports-add').addEventListener('click', () => {
        if (!teamSelect.reportValidity()) return;
        selectedTeam = teamSelect.value;
        updateSports();
        bootstrap.Modal.getOrCreateInstance(modal).hide();
    });
    document.getElementById('sports-remove').addEventListener('click', () => { selectedTeam = ''; updateSports(); });
})();
