(() => {
    'use strict';
    const form = document.getElementById('carteirinha-form');
    if (!form) return;
    const steps = [...form.querySelectorAll('[data-step]')];
    const indicators = [...document.querySelectorAll('.carteirinha-steps li')];
    const next = document.getElementById('step-next');
    const back = document.getElementById('step-back');
    const review = document.getElementById('carteirinha-review');
    let current = 0;
    const dependentesToggle = document.getElementById('incluir-dependentes');
    const dependentesContainer = document.getElementById('dependentes-carteirinha');
    const dependentesList = document.getElementById('dependentes-carteirinha-lista');
    const dependentesTemplate = document.getElementById('dependente-carteirinha-template');
    const dependentesAdd = document.getElementById('adicionar-dependente-carteirinha');
    const valorPorDependente = 11.50;
    let intencaoCriada = false;
    const field = name => form.elements.namedItem(name);
    const digits = value => value.replace(/\D/g, '');
    const displayBirthdate = () => field('dataNascimento').value.split('-').reverse().join('/');
    function maskPhone(input) {
        const caret = input.selectionStart;
        const digitsBeforeCaret = digits(input.value.slice(0, caret ?? input.value.length)).length;
        const phone = digits(input.value).slice(0, 11);
        let formatted = phone;
        if (phone.length > 2) {
            const local = phone.slice(2);
            const split = phone.length > 10 ? 5 : 4;
            formatted = `(${phone.slice(0, 2)}) ${local.slice(0, split)}${local.length > split ? '-' + local.slice(split) : ''}`;
        }
        input.value = formatted;
        if (document.activeElement === input && caret !== null) {
            let position = 0;
            let count = 0;
            while (position < formatted.length && count < digitsBeforeCaret) {
                if (/\d/.test(formatted[position])) count++;
                position++;
            }
            input.setSelectionRange(position, position);
        }
    }
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
        if (section.querySelector('[data-dependent="cpf"]')) validarDocumentos();
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
        const total = calcularTotal();
        const values = [
            ['Nome', field('nome').value.trim()], ['E-mail', field('email').value.trim()],
            ['WhatsApp', field('telefone').value], ['CPF', field('cpf').value],
            ['Data de nascimento', displayBirthdate()],
            ['Plano', field('planoId').selectedOptions[0]?.textContent || 'Não selecionado'],
            ['Total da carteirinha', formatarMoeda(total)],
            ['Endereço', `${field('rua').value}, ${field('numero').value} — ${field('bairro').value}, ${field('cidade').value}/${field('uf').value}, CEP ${field('cep').value}`],
            ['Complemento', field('complemento').value || 'Não informado'],
            ['Dependentes', dependentesToggle.checked ? `${dependentesList.children.length} incluído(s)` : 'Não incluídos'], ['Pagamento', field('pagamento').value],
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
        document.getElementById('preview-name').textContent = field('nome').value.trim() || 'Seu nome completo';
        document.getElementById('preview-birthdate').textContent = displayBirthdate() || '— / — / —';
    });
    field('telefone').addEventListener('input', event => maskPhone(event.target));
    field('telefone').addEventListener('change', event => maskPhone(event.target));
    maskPhone(field('telefone'));
    field('cpf').addEventListener('input', event => {
        event.target.value = digits(event.target.value).slice(0, 11).replace(/^(\d{3})(\d)/, '$1.$2').replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3').replace(/(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4');
    });
    field('cep').addEventListener('input', event => { event.target.value = digits(event.target.value).slice(0, 8).replace(/^(\d{5})(\d)/, '$1-$2'); });
    const cepStatus = document.getElementById('carteirinha-cep-status');
    let cepRequest;
    let cepRevision = 0;
    field('cep').addEventListener('input', () => {
        cepRevision++;
        cepRequest?.abort();
        cepStatus.textContent = '';
    });
    field('cep').addEventListener('blur', async () => {
        const cep = digits(field('cep').value);
        cepRequest?.abort();
        const revision = ++cepRevision;
        if (!cep) { cepStatus.textContent = ''; return; }
        if (cep.length !== 8) {
            cepStatus.textContent = 'Informe um CEP com 8 dígitos.';
            return;
        }
        const controller = new AbortController();
        cepRequest = controller;
        const timeout = window.setTimeout(() => controller.abort(), 10000);
        const addressFields = { rua: 'logradouro', bairro: 'bairro', cidade: 'localidade', uf: 'uf' };
        const initialValues = Object.fromEntries(Object.keys(addressFields).map(name => [name, field(name).value]));
        cepStatus.textContent = 'Consultando CEP...';
        try {
            const response = await fetch(`https://viacep.com.br/ws/${cep}/json/`, { signal: controller.signal });
            if (!response.ok) throw new Error('Consulta indisponível');
            const address = await response.json();
            if (revision !== cepRevision) return;
            if (address.erro || !address.localidade || !address.uf) {
                cepStatus.textContent = 'CEP não encontrado. Confira o CEP ou preencha o endereço manualmente.';
                return;
            }
            for (const [name, property] of Object.entries(addressFields)) {
                const input = field(name);
                // Preserve manual edits made while the lookup was pending.
                if (input.value !== initialValues[name]) continue;
                input.value = address[property] || '';
                input.setCustomValidity('');
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }
            cepStatus.textContent = 'CEP consultado. Confira o endereço e complete os campos restantes.';
        } catch {
            if (revision === cepRevision)
                cepStatus.textContent = 'Não foi possível consultar o CEP. Preencha o endereço manualmente.';
        } finally {
            window.clearTimeout(timeout);
            if (revision === cepRevision) cepRequest = null;
        }
    });
    form.querySelectorAll('[name="pagamento"]').forEach(input => input.addEventListener('change', () => {
        document.getElementById('summary-payment').textContent = input.value;
        document.getElementById('payment-note').textContent = input.value === 'Pix'
            ? 'Pix selecionado. O código de pagamento será disponibilizado quando a cobrança online estiver habilitada.'
            : `${input.value} selecionado. Os dados do cartão serão solicitados no ambiente de pagamento quando a cobrança estiver habilitada.`;
    }));
    document.getElementById('summary-payment').textContent = 'Pix';
    async function registrarIntencaoVenda() {
        if (intencaoCriada) return;
        const dependentes = [...dependentesList.querySelectorAll('.carteirinha-dependent')].map(item => ({
            nome: item.querySelector('[data-dependent="nome"]')?.value || '',
            cpf: item.querySelector('[data-dependent="cpf"]')?.value || '',
            nascimento: item.querySelector('[data-dependent="nascimento"]')?.value || '',
            parentesco: item.querySelector('[data-dependent="parentesco"]')?.value || ''
        }));
        const payload = {
            nome: field('nome').value.trim(), email: field('email').value.trim(), telefone: field('telefone').value,
            cpf: field('cpf').value, planoId: field('planoId').value,
            quantidadeDependentes: dependentes.length,
            endereco: `${field('rua').value}, ${field('numero').value}, ${field('bairro').value}, ${field('cidade').value}/${field('uf').value}, CEP ${field('cep').value}`,
            dependentes: JSON.stringify(dependentes)
        };
        const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const response = await fetch('/Carteirinha/CriarIntencao', { method: 'POST', headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token || '' }, body: JSON.stringify(payload) });
        if (!response.ok) throw new Error('Não foi possível registrar a solicitação.');
        intencaoCriada = true;
    }
    document.getElementById('copiar-pix')?.addEventListener('click', async () => {
        const chave = document.getElementById('carteirinha-pix-chave').textContent.trim();
        const status = document.getElementById('pix-status');
        let copiada = false;
        try {
            await navigator.clipboard.writeText(chave);
            copiada = true;
        } catch {
            // Alguns navegadores bloqueiam o clipboard; o clique ainda deve registrar a intenção.
        }
        try {
            await registrarIntencaoVenda();
            status.textContent = copiada
                ? 'Chave Pix copiada. Solicitação registrada como aguardando pagamento.'
                : 'Solicitação registrada como aguardando pagamento. Copie a chave Pix manualmente.';
        } catch {
            status.textContent = 'Não foi possível registrar a solicitação. Verifique os dados e tente novamente.';
        }
    });
    function formatarMoeda(valor) {
        return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
    }
    function valorPlanoSelecionado() {
        const texto = field('planoId').selectedOptions[0]?.dataset.valor || '';
        const valor = Number(texto.replace(/[^\d,.-]/g, '').replace(/\./g, '').replace(',', '.'));
        return Number.isFinite(valor) ? valor : 0;
    }
    function calcularTotal() {
        const totalDependentes = dependentesToggle.checked ? dependentesList.children.length * valorPorDependente : 0;
        return valorPlanoSelecionado() + totalDependentes;
    }
    function atualizarResumoValores() {
        const totalDependentes = dependentesToggle.checked ? dependentesList.children.length * valorPorDependente : 0;
        document.getElementById('summary-plano-valor').textContent = formatarMoeda(valorPlanoSelecionado());
        document.getElementById('summary-dependentes-total').textContent = formatarMoeda(totalDependentes);
        document.getElementById('summary-total-carteirinha').textContent = formatarMoeda(calcularTotal());
    }
    function validarDocumentos() {
        const titular = digits(field('cpf').value);
        const vistos = new Set();
        dependentesList.querySelectorAll('[data-dependent="cpf"]').forEach(input => {
            input.setCustomValidity('');
            const cpf = digits(input.value);
            if (!validCpf(cpf)) input.setCustomValidity('Informe um CPF válido.');
            else if (cpf === titular) input.setCustomValidity('O CPF do dependente não pode ser igual ao do titular.');
            else if (vistos.has(cpf)) input.setCustomValidity('Cada dependente deve ter um CPF diferente.');
            vistos.add(cpf);
        });
    }
    function updateDependentes() {
        dependentesContainer.hidden = !dependentesToggle.checked;
        dependentesContainer.querySelectorAll('input, select').forEach(input => input.disabled = !dependentesToggle.checked);
        dependentesAdd.disabled = !dependentesToggle.checked || dependentesList.children.length >= 5;
        document.getElementById('dependentes-limite-carteirinha').hidden = dependentesList.children.length < 5;
        document.getElementById('summary-dependentes').textContent = dependentesToggle.checked && dependentesList.children.length
            ? `${dependentesList.children.length} incluído(s)` : 'Nenhum';
        atualizarResumoValores();
    }
    function addDependente() {
        if (dependentesList.children.length >= 5) return updateDependentes();
        dependentesList.append(dependentesTemplate.content.cloneNode(true));
        updateDependentes();
    }
    dependentesToggle.addEventListener('change', () => {
        if (dependentesToggle.checked && !dependentesList.children.length) addDependente();
        updateDependentes();
    });
    field('planoId').addEventListener('change', event => {
        document.getElementById('summary-plano').textContent = event.target.selectedOptions[0]?.textContent || 'A escolher';
    });
    dependentesAdd.addEventListener('click', addDependente);
    dependentesList.addEventListener('click', event => {
        if (!event.target.matches('[data-dependent="remover"]')) return;
        event.target.closest('.carteirinha-dependent').remove();
        updateDependentes();
    });
    dependentesList.addEventListener('input', event => {
        if (event.target.matches('[data-dependent="cpf"]')) {
            event.target.value = digits(event.target.value).slice(0, 11).replace(/^(\d{3})(\d)/, '$1.$2').replace(/^(\d{3})\.(\d{3})(\d)/, '$1.$2.$3').replace(/(\d{3})\.(\d{3})\.(\d{3})(\d)/, '$1.$2.$3-$4');
            validarDocumentos();
        }
    });
    updateDependentes();
})();
