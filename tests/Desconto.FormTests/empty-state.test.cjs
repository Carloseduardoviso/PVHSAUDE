const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');

const view = readFileSync(join(__dirname, '../../src/Web/Areas/Administracao/Views/Desconto/Index.cshtml'), 'utf8');

test('exibe mensagem quando não existem descontos cadastrados', () => {
    assert.match(view, /Model\.Count == 0/);
    assert.match(view, /Nenhum desconto cadastrado\./);
});
