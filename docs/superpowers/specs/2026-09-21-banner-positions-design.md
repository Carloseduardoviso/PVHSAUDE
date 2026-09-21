# Banners por posição no portal

## Objetivo

Permitir que o administrador cadastre vários banners ativos para cada área visual
do portal e que cada área execute seu próprio carrossel, sem alterar a stack
Razor, ASP.NET Core ou Bootstrap já adotada.

## Escopo

Cada banner terá uma posição obrigatória, escolhida no cadastro administrativo:

1. `LateralEsquerda` — banner vertical à esquerda.
2. `Central` — banner paisagem no topo central, proporção 16:5; recomendação
   de 1600 × 500 px.
3. `LateralDireita` — banner vertical à direita.
4. `InferiorEsquerda` — banner horizontal abaixo, à esquerda.
5. `InferiorDireita` — banner horizontal abaixo, à direita.

Cada posição aceita vários banners ativos. O portal agrupa os banners por
posição e exibe um carrossel independente para cada grupo. Uma posição sem
banners não renderiza um espaço vazio.

## Arquitetura e fluxo de dados

Um enum de domínio representa as cinco posições. A entidade, o ViewModel, o
AutoMapper, os contratos da API e a persistência passam a transportar esse
campo. Banners legados receberão `Central` como padrão para preservar o portal
atual após a migração.

O formulário administrativo mostra um `select` obrigatório com os nomes
amigáveis das posições. A listagem também exibe a posição escolhida para evitar
ambiguidade na administração.

No portal, a ação inicial solicita os banners ativos uma vez, agrupa-os por
posição e entrega os grupos ao ViewModel. Um partial reutilizável renderiza um
carrossel Bootstrap por área, com IDs exclusivos. O JavaScript existente do
carrossel será ajustado para inicializar todos os carrosséis da página.

## Layout responsivo

Em telas largas, a composição forma três colunas no topo: lateral esquerda,
centro 16:5 e lateral direita. A faixa inferior possui os dois banners
horizontais. Em larguras menores, a grade se adapta para duas colunas e, em
celulares, as áreas empilham em ordem: central, laterais, inferiores. Nenhuma
área exige menu hambúrguer ou altera a navegação existente.

Imagens usam `object-fit: contain`, preservando a peça enviada pelo
administrador sem corte. Cada posição receberá uma proporção CSS apropriada,
com fundo neutro para espaços residuais de imagem.

## Validação e falhas

O servidor rejeita posições ausentes ou inválidas. A ausência de banners em
uma posição é estado normal: o portal apenas omite a área. Uma falha de API
mantém a página carregável, como ocorre hoje; os carrosséis disponíveis não
dependem uns dos outros.

## Testes

- Serviço/API: posição é criada, editada, lida e filtrada com o banner.
- Formulário administrativo: valida a posição obrigatória e preserva a seleção
  em caso de erro.
- Portal: separa banners ativos em cada posição e não renderiza regiões vazias.
- Cliente: cada carrossel recebe um identificador exclusivo e controles que
  atuam somente no grupo correspondente.
- Regressão: banners existentes sem posição continuam no carrossel central.

## Fora de escopo

Não inclui redimensionamento automático de imagens, criação de novos formatos
de arquivo, alteração de storage, campanhas temporizadas, métricas, mudança de
stack ou migrações fora do campo de posição necessário.
