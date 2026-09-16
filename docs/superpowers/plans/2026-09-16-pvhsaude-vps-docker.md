# PVHSAUDE VPS Docker Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox syntax for tracking.

**Goal:** Publicar PVHSAUDE.Web e PVHSAUDE.Api com SQL Server, HTTPS e reinicialização automática na VPS Hostinger.

**Architecture:** Caddy expõe somente 80/443 e encaminha HTTPS para PVHSAUDE.Web. Web e API usam imagens .NET 10 e rede Docker privada; SQL Server tem volume persistente e nenhuma porta pública.

**Tech Stack:** .NET 10, ASP.NET Core MVC/API, Docker Compose, SQL Server 2022 Express, Caddy 2, Ubuntu 24.04.

**Spec:** docs/superpowers/specs/2026-09-16-pvhsaude-vps-docker-design.md

## Global Constraints

- Não alterar regras de negócio nem modernizar a stack existente.
- Não versionar senha SQL Server, segredos JWT ou .env de produção.
- Expor apenas 80 e 443; SQL Server, API e Web ficam privados.
- Usar pvhsaude.com.br como domínio canônico e redirecionar www.
- Não aplicar cadastro-usuarios.sql sem confirmar que corresponde ao esquema.

---

## File Structure

- src/Web/Dockerfile: build multi-stage e runtime da Web.
- src/Api/Dockerfile: build multi-stage e runtime da API.
- src/Web/Program.cs e src/Api/Program.cs: proxy e endpoint /health.
- compose.yaml: Caddy, Web, API, SQL Server, rede, volumes e health checks.
- Caddyfile: TLS, domínio canônico e proxy.
- .env.example e .dockerignore: configuração segura de build/deploy.
- scripts/backup-sqlserver.sh: backup persistente.
- docs/deploy-vps.md: operação, backup e rollback.

### Task 1: Preparar Web e API para proxy e saúde

**Files:**
- Modify: src/Web/Program.cs
- Modify: src/Api/Program.cs
- Create: tests/Deployment/health-checks.md

**Interfaces:**
- Produces: GET /health retorna HTTP 200 sem autenticação em Web e API.
- Produces: ambos respeitam X-Forwarded-For e X-Forwarded-Proto enviados pelo Caddy.

- [ ] **Step 1: Criar especificação de teste**

Criar tests/Deployment/health-checks.md:

~~~~markdown
# Health checks de deploy
- http://web:8080/health retorna 200.
- http://api:8080/health retorna 200.
- A URL pública HTTPS não entra em loop de redirecionamento.
~~~~

- [ ] **Step 2: Adicionar endpoint em ambos os projetos**

Antes de app.Run(); inserir:

~~~~csharp
app.MapGet("/health", () => Results.Ok()).AllowAnonymous();
~~~~

- [ ] **Step 3: Configurar proxy encaminhado**

Adicionar using Microsoft.AspNetCore.HttpOverrides;, a configuração abaixo após criar builder, e app.UseForwardedHeaders(); antes de app.UseHttpsRedirection(); em ambos os projetos.

~~~~csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
~~~~

- [ ] **Step 4: Compilar**

Run:

~~~~powershell
dotnet build src/Web/PVHSAUDE.Web.csproj -c Release
dotnet build src/Api/PVHSAUDE.Api.csproj -c Release
~~~~

Expected: os dois comandos finalizam com Build succeeded.

- [ ] **Step 5: Commit**

~~~~bash
git add src/Web/Program.cs src/Api/Program.cs tests/Deployment/health-checks.md
git commit -m "feat: add deployment health endpoints"
~~~~

### Task 2: Criar Dockerfiles e variáveis de produção

**Files:**
- Create: src/Web/Dockerfile
- Create: src/Api/Dockerfile
- Create: .dockerignore
- Create: .env.example

**Interfaces:**
- Produces: imagens que escutam em 8080.
- Produces: configuração por Api__BaseUrl, ConnectionStrings__DefaultConnection e Jwt__*.

- [ ] **Step 1: Criar exclusões de build**

Criar .dockerignore:

~~~~gitignore
**/bin/
**/obj/
.git/
.vs/
.env
~~~~

- [ ] **Step 2: Criar Dockerfiles multi-stage**

Usar mcr.microsoft.com/dotnet/sdk:10.0 para restore/publish e mcr.microsoft.com/dotnet/aspnet:10.0 para runtime. Cada imagem publica seu projeto em /app/publish, define ASPNETCORE_URLS=http://+:8080, expõe 8080 e executa seu DLL.

- [ ] **Step 3: Criar .env.example**

~~~~dotenv
MSSQL_SA_PASSWORD=troque-por-senha-forte-com-mais-de-8-caracteres
JWT__KEY=troque-por-chave-aleatoria-de-producao
JWT__ISSUER=pvhsaude.com.br
JWT__AUDIENCE=pvhsaude.com.br
~~~~

- [ ] **Step 4: Validar imagens**

Run:

~~~~powershell
docker build -f src/Web/Dockerfile -t pvhsaude-web:local .
docker build -f src/Api/Dockerfile -t pvhsaude-api:local .
~~~~

Expected: ambas as imagens são criadas sem erro.

- [ ] **Step 5: Commit**

~~~~bash
git add .dockerignore .env.example src/Web/Dockerfile src/Api/Dockerfile
git commit -m "build: containerize web and api"
~~~~

### Task 3: Orquestrar Caddy, Web, API e SQL Server

**Files:**
- Create: compose.yaml
- Create: Caddyfile
- Create: scripts/backup-sqlserver.sh

**Interfaces:**
- Consumes: Dockerfiles e variáveis da Task 2.
- Produces: domínio público para Web; http://api:8080 para Web; sqlserver,1433 para API.

- [ ] **Step 1: Criar Compose de produção**

Criar serviços caddy, web, api e sqlserver; volumes caddy_data, caddy_config, sqlserver_data e sqlserver_backups; rede interna pvhsaude. Aplicar restart: unless-stopped e health checks.

O SQL Server usa:

~~~~yaml
image: mcr.microsoft.com/mssql/server:2022-latest
environment:
  ACCEPT_EULA: "Y"
  MSSQL_PID: Express
  MSSQL_SA_PASSWORD: ${MSSQL_SA_PASSWORD}
volumes:
  - sqlserver_data:/var/opt/mssql
  - sqlserver_backups:/var/opt/mssql/backups
~~~~

A Web usa Api__BaseUrl: http://api:8080/. A API usa:

~~~~yaml
ConnectionStrings__DefaultConnection: Server=sqlserver,1433;Database=PVHSAUDE;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;Encrypt=False
Jwt__Key: ${JWT__KEY}
Jwt__Issuer: ${JWT__ISSUER}
Jwt__Audience: ${JWT__AUDIENCE}
~~~~

Somente Caddy publica 80:80 e 443:443. SQL usa health check sqlcmd -C; Web/API usam /health.

- [ ] **Step 2: Criar Caddyfile**

~~~~caddyfile
www.pvhsaude.com.br {
    redir https://pvhsaude.com.br{uri} permanent
}

pvhsaude.com.br {
    reverse_proxy web:8080
}
~~~~

- [ ] **Step 3: Criar backup do SQL Server**

Criar scripts/backup-sqlserver.sh para executar sqlcmd no serviço sqlserver e salvar PVHSAUDE-<UTC>.bak em /var/opt/mssql/backups com BACKUP DATABASE [PVHSAUDE].

- [ ] **Step 4: Validar Compose**

Run:

~~~~powershell
docker compose config
docker compose up --build -d
docker compose ps
docker compose logs --tail=100 web api sqlserver caddy
~~~~

Expected: quatro serviços em execução; Web/API/SQL saudáveis.

- [ ] **Step 5: Verificar endpoints privados**

Run:

~~~~powershell
docker compose exec web wget -qO- http://localhost:8080/health
docker compose exec api wget -qO- http://localhost:8080/health
~~~~

Expected: ambos retornam 200.

- [ ] **Step 6: Validar schema antes de carga**

Ler cadastro-usuarios.sql e comparar dependências com o banco vazio. Aplicar somente se tabelas e relações necessárias existirem; se não, interromper e registrar a dependência em docs/deploy-vps.md.

- [ ] **Step 7: Commit**

~~~~bash
git add compose.yaml Caddyfile scripts/backup-sqlserver.sh
git commit -m "feat: add production compose stack"
~~~~

### Task 4: Preparar VPS, DNS e publicar

**Files:**
- Create: docs/deploy-vps.md

**Interfaces:**
- Consumes: artefatos das Tasks 1 a 3 e hPanel Hostinger autenticado.
- Produces: deploy em /opt/pvhsaude e TLS válido.

- [ ] **Step 1: Preparar acesso e firewall**

Atualizar VPS, criar usuário deploy, adicionar ao grupo Docker e permitir SSH, HTTP e HTTPS. Confirmar SSH por chave antes de desabilitar root por senha.

- [ ] **Step 2: Apontar DNS**

Criar registros A de @ e www para o IP público da VPS e aguardar resolução antes de iniciar Caddy.

- [ ] **Step 3: Instalar projeto**

~~~~bash
sudo mkdir -p /opt/pvhsaude
sudo chown deploy:deploy /opt/pvhsaude
git clone https://github.com/Carloseduardoviso/PVHSAUDE.git /opt/pvhsaude
cd /opt/pvhsaude
cp .env.example .env
chmod 600 .env
~~~~

Preencher .env diretamente na VPS; nunca enviar segredos ao Git ou chat.

- [ ] **Step 4: Publicar e validar domínio**

~~~~bash
cd /opt/pvhsaude
docker compose up --build -d
docker compose ps
curl -I https://pvhsaude.com.br/health
curl -I https://www.pvhsaude.com.br/health
~~~~

Expected: domínio canônico retorna 200; www redireciona permanentemente.

- [ ] **Step 5: Agendar e testar backup**

Agendar scripts/backup-sqlserver.sh diariamente no cron de deploy, executar uma vez e confirmar arquivo .bak no volume de backup.

- [ ] **Step 6: Documentar operação**

Criar docs/deploy-vps.md com status, logs, atualização, backup, restauração e rollback pelo commit anterior.

- [ ] **Step 7: Commit**

~~~~bash
git add docs/deploy-vps.md
git commit -m "docs: document VPS deployment operations"
~~~~

## Self-review

- Cobertura: Tasks 1-3 implementam Web/API/SQL/Caddy, segredos, persistência, health checks e backup; Task 4 cobre DNS, firewall, deploy e aceite público.
- Sem placeholders: arquivos, nomes de serviço, variáveis, comandos e validações são definidos.
- Consistência: api, web, sqlserver e caddy são os mesmos nomes em Compose, Caddyfile e validações.

