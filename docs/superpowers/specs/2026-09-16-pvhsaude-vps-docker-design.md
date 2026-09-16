# Deploy do PVHSAUDE em VPS Docker

## Objetivo

Publicar o sistema PVHSAUDE em uma VPS Hostinger Ubuntu 24.04, usando o
domínio `pvhsaude.com.br`, HTTPS automático e containers reiniciáveis.

## Topologia aprovada

```text
Internet (80/443)
        |
      Caddy
        |
  PVHSAUDE.Web <--> PVHSAUDE.Api <--> SQL Server
```

- Caddy é o único serviço que publica portas na internet. Ele atenderá
  `pvhsaude.com.br`, redirecionará `www.pvhsaude.com.br` e obterá/renovará os
  certificados TLS.
- Web e API são imagens .NET 10 separadas e não expõem portas no host. A Web
  chama a API pelo nome do serviço Docker, na rede privada do Compose.
- SQL Server fica em container sem porta publicada, com volume nomeado para
  persistência de dados.
- O Compose usa políticas `restart: unless-stopped`, health checks e uma rede
  privada para todos os serviços.

## Alterações no repositório

- Criar Dockerfile multi-stage para `src/Web/PVHSAUDE.Web.csproj`.
- Criar Dockerfile multi-stage para `src/Api/PVHSAUDE.Api.csproj`.
- Criar `compose.yaml` de produção, `Caddyfile` e `.env.example` sem segredos.
- Configurar `Api__BaseUrl` da Web para o endereço interno da API no Compose.
- Configurar cabeçalhos encaminhados para permitir HTTPS atrás do proxy sem
  loop de redirecionamento.
- Manter segredos fora do Git: senha SQL Server e valores JWT existirão apenas
  no `.env` da VPS.

## Banco de dados

- O SQL Server será inicializado com banco e volume persistente.
- O repositório não possui migrations do Entity Framework. Antes de liberar o
  sistema, será validado e aplicado o script `cadastro-usuarios.sql` quando ele
  corresponder ao esquema necessário; qualquer esquema adicional será tratado
  como bloqueio, sem inventar dados ou tabelas.
- O backup inicial será um dump agendado para diretório persistente na VPS. A
  cópia externa do backup permanece uma etapa posterior e recomendada.

## DNS e segurança

- Criar registros A de `pvhsaude.com.br` e `www` para o IP público da VPS.
- Liberar somente SSH administrativo, HTTP e HTTPS; não publicar SQL Server,
  API ou Web diretamente.
- Criar usuário de deploy e preferir chave SSH antes de desabilitar acesso root
  por senha, após confirmar um segundo acesso funcional.

## Verificação de aceite

1. As imagens Web e API compilam com .NET 10.
2. `docker compose up -d` mantém os quatro serviços saudáveis.
3. `https://pvhsaude.com.br` responde com certificado válido.
4. A Web se comunica com a API sem endereço localhost externo.
5. SQL Server não pode ser acessado pela internet e os dados sobrevivem ao
   reinício dos containers.
