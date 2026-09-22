# Página Sobre Nós

## Objetivo

Apresentar o PVH Saúde + como cartão de descontos e benefícios de Porto Velho, destacando terapias, rede de saúde, economia e cuidado familiar em uma página acolhedora, legível e responsiva.

## Estrutura

1. **Hero**: título “Sua saúde começa aqui”, resumo da proposta e imagem institucional de uma família brasileira em contexto de cuidado e bem-estar. A imagem não terá texto, marcas nem marcas-d’água.
2. **Nossa história**: texto fornecido reorganizado em parágrafos curtos. Explica a origem no Grupo Kaheli, a estrutura terapêutica própria e a razão social do PVH Saúde +.
3. **Cuidado que alcança a família toda**: cartões visuais para Terapias essenciais, Saúde em rede, Economia no dia a dia e Educação, alimentação e lazer. Os cartões usarão ícones SVG inline para não depender de mais imagens.
4. **Fechamento**: destaque para valores fixos em clínicas credenciadas, descontos de até 40% e a mensagem “Sua saúde começa aqui.”

## Imagem

Será gerada uma imagem horizontal fotorealista, sem texto: família brasileira diversa em uma sala de terapia infantil clara e acolhedora, com profissional de saúde ao fundo, luz natural e tons verdes e azuis discretos. O arquivo será salvo em `wwwroot/images/` e exibido com `object-fit: cover` em um quadro responsivo.

## Responsividade e acessibilidade

A página terá largura de leitura confortável, hero em duas colunas em telas grandes e uma coluna em telas menores. A imagem terá `alt` descritivo; os blocos serão seções semânticas com títulos hierárquicos. Não haverá informação essencial somente em imagem.

## Testes

Um teste de formato verificará que a view contém as seções, a chamada para ação e a referência ao ativo. O teste atual da Home será executado com a suite de formulários.

## Fora de escopo

Não haverá formulário novo, alteração de planos, integração externa ou mudança de navegação.