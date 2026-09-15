const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');
const { runInNewContext } = require('node:vm');

const script = readFileSync(join(__dirname, '../../src/Web/wwwroot/js/plano-form.js'), 'utf8');

function form(today, period, expiration = '') {
    const handlers = {};
    const periodicidade = { value: period, addEventListener: (event, handler) => { handlers[event] = handler; } };
    const validade = { value: expiration };
    class Clock extends Date {
        constructor(...args) { super(...(args.length ? args : today)); }
    }
    runInNewContext(script, {
        Date: Clock,
        window: {},
        document: {
            querySelectorAll: () => [],
            getElementById: id => ({ Periodicidade: periodicidade, DataValidade: validade })[id]
        }
    });
    return {
        validade,
        change(value) { periodicidade.value = value; handlers.change?.(); }
    };
}

test('mensal e anual preenchem um ano a partir de hoje', () => {
    for (const period of ['0', '3'])
        assert.equal(form([2026, 8, 15], period).validade.value, '2027-09-15');
});

test('semestral preenche seis meses, inclusive na virada do ano', () => {
    assert.equal(form([2026, 8, 15], '2').validade.value, '2027-03-15');
});

test('usa o último dia do mês quando o dia de origem não existe no destino', () => {
    assert.equal(form([2024, 1, 29], '3').validade.value, '2025-02-28');
    assert.equal(form([2026, 7, 31], '2').validade.value, '2027-02-28');
    assert.equal(form([2027, 7, 31], '2').validade.value, '2028-02-29');
});

test('preserva validade existente ao abrir e recalcula quando muda a periodicidade', () => {
    const current = form([2026, 8, 15], '0', '2030-01-10');
    assert.equal(current.validade.value, '2030-01-10');
    current.change('2');
    assert.equal(current.validade.value, '2027-03-15');
    current.change('3');
    assert.equal(current.validade.value, '2027-09-15');
    current.change('0');
    assert.equal(current.validade.value, '2027-09-15');
});

test('trimestral preenche três meses a partir de hoje', () => {
    assert.equal(form([2026, 8, 15], '1').validade.value, '2026-12-15');
    const current = form([2026, 8, 15], '0', '2030-01-10');
    current.change('1');
    assert.equal(current.validade.value, '2026-12-15');
});
