const { test } = require('node:test');
const assert = require('node:assert/strict');
const { readFileSync } = require('node:fs');
const { join } = require('node:path');

const form = readFileSync(join(__dirname, '../../src/Web/Areas/Administracao/Views/Desconto/_Form.cshtml'), 'utf8');
const create = readFileSync(join(__dirname, '../../src/Web/Areas/Administracao/Views/Desconto/Create.cshtml'), 'utf8');
const edit = readFileSync(join(__dirname, '../../src/Web/Areas/Administracao/Views/Desconto/Edit.cshtml'), 'utf8');

test('formulário de desconto usa máscara de moeda no valor', () => {
    assert.match(form, /data-mask="moeda"/);
    assert.match(form, /data-rule-moeda="true"/);
    assert.match(create, /js\/plano-form\.js/);
    assert.match(edit, /js\/plano-form\.js/);
});
