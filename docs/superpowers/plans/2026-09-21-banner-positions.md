# Banners por posição no portal Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Permitir carrosséis independentes de banners nas cinco áreas do portal e selecionar a área no cadastro administrativo.

**Architecture:** Um enum de domínio `PosicaoBanner` será persistido no banner e exposto pelos ViewModels/API. A página inicial agrupará os banners ativos por posição e um partial reutilizável renderizará um carrossel para cada grupo não vazio; CSS define a grade responsiva e o JavaScript inicializa todos os carrosséis.

**Tech Stack:** .NET 10, ASP.NET Core MVC/Razor, Entity Framework Core, SQL Server, Bootstrap, JavaScript sem framework.

**Spec:** `docs/superpowers/specs/2026-09-21-banner-positions-design.md`

## Global Constraints

- Preservar ASP.NET Core MVC, Razor, Bootstrap e JavaScript existente; não introduzir dependências.
- Cinco posições: `LateralEsquerda`, `Central`, `LateralDireita`, `InferiorEsquerda`, `InferiorDireita`.
- Banners existentes devem receber `Central` na migração.
- Aceitar vários banners ativos por posição, com carrossel independente.
- Não executar migrations contra banco real nem operações Git sem solicitação explícita do usuário.
- Não criar áreas vazias quando uma posição não tiver banner ativo.

## Review Focus

- Banner legado sem posição deve aparecer no carrossel central após a migração.
- Uma posição sem banners ativos não deve deixar coluna ou espaço em branco no portal.
- Dois carrosséis diferentes devem ter IDs, indicadores e controles independentes.
- Imagem fora da proporção própria da posição deve ser rejeitada antes do envio e pelo servidor.
- Em celular, as cinco áreas devem continuar acessíveis em coluna, sem sobreposição ou conteúdo oculto.

---

### Task 1: Modelo de posição, persistência e contratos da API

**Files:**
- Create: `src/Domain/Enuns/PosicaoBanner.cs`
- Modify: `src/Domain/Entities/Banner.cs`
- Modify: `src/Application/ViewModels/BannerVm.cs`
- Modify: `src/Web/Models/BannerVm.cs`
- Modify: `src/Infra/Infra.Data/Config/BannerConfig.cs`
- Create: `src/Infra/Infra.Data/Migrations/20260921100000_PosicaoBanner.cs`
- Create: `src/Infra/Infra.Data/Migrations/20260921100000_PosicaoBanner.Designer.cs`
- Modify: `src/Infra/Infra.Data/Migrations/ContextModelSnapshot.cs`
- Modify: `src/Application/AutoMapper/AutoMapperConfig.cs`
- Test: `tests/Usuario.FormTests/BannerApiClientTests.cs`

**Interfaces:**
- Produces: `enum PosicaoBanner { LateralEsquerda = 1, Central = 2, LateralDireita = 3, InferiorEsquerda = 4, InferiorDireita = 5 }`.
- Produces: `Banner.Posicao` and both `BannerVm.Posicao` properties of type `PosicaoBanner`.
- Produces: an EF migration adding non-null `Posicao` (`int`) with default `Central` to `Banner`.

- [ ] **Step 1: Write failing API-client assertions**

```csharp
var banner = new BannerVm { Id = Guid.NewGuid(), Titulo = "Lateral", Ativo = true,
    Posicao = PosicaoBanner.LateralEsquerda };
var lista = await client.ListarAsync(true, default);
Check(lista.Single().Posicao == PosicaoBanner.LateralEsquerda,
    "listagem preserva a posição enviada pela API");
```

- [ ] **Step 2: Run the banner client test and verify it fails**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Expected: compilation failure because `PosicaoBanner`/`Posicao` do not exist.

- [ ] **Step 3: Add the enum and propagate it through entity, ViewModels, mapping and EF configuration**

```csharp
public enum PosicaoBanner
{
    LateralEsquerda = 1,
    Central = 2,
    LateralDireita = 3,
    InferiorEsquerda = 4,
    InferiorDireita = 5
}

public PosicaoBanner Posicao { get; set; } = PosicaoBanner.Central;
```

Configure `b.Property(x => x.Posicao).IsRequired();`. Generate an EF migration that adds `Posicao` with default value `(int)PosicaoBanner.Central` so existing rows remain visible no centro.

- [ ] **Step 4: Run the banner client test and verify it passes**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Expected: `PASS: Banner - listagem preserva a posição enviada pela API` plus existing tests.

### Task 2: Validação de formato por posição e cadastro administrativo

**Files:**
- Modify: `src/Application/AppService/BannerFormato.cs`
- Modify: `src/Application/AppService/BannerService.cs`
- Modify: `src/Web/Areas/Administracao/Views/Banner/Form.cshtml`
- Modify: `src/Web/Areas/Administracao/Views/Banner/Index.cshtml`
- Modify: `src/Web/wwwroot/js/banner-form.js`
- Test: `tests/Usuario.FormTests/BannerApiClientTests.cs`

**Interfaces:**
- Consumes: `BannerVm.Posicao` from Task 1.
- Produces: `BannerFormato.Valido(byte[] imagem, PosicaoBanner posicao)` and position-specific validation messages.
- Produces: form field `select[name="Posicao"]` with all five enum values.

- [ ] **Step 1: Write failing tests for position-specific validation**

```csharp
Check(!BannerFormato.Valido(PngDe1600Por500, PosicaoBanner.LateralEsquerda),
    "imagem central não é aceita como lateral vertical");
Check(BannerFormato.Valido(PngDe1600Por500, PosicaoBanner.Central),
    "imagem 16:5 é aceita no centro");
```

- [ ] **Step 2: Run the relevant form test and verify it fails**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Expected: missing overload or assertion failure because format validation is currently always 16:5.

- [ ] **Step 3: Implement the position selector and validation**

Use `Html.GetEnumSelectList<PosicaoBanner>()` in the form. Keep central at 16:5 (1600 × 500 recommended); add documented ratio checks for vertical and horizontal slots. Pass `vm.Posicao` to `BannerFormato.Valido`. In `banner-form.js`, read `#Posicao` and validate the selected image using the same ratio map; update `#imagem-ajuda` when selection changes. Display `Posicao` in desktop table and mobile card list.

- [ ] **Step 4: Run the form test and verify it passes**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Expected: position validation assertions and existing banner API-client tests pass.

### Task 3: Agrupamento de banners e composição do portal

**Files:**
- Modify: `src/Web/Models/PortalVm.cs`
- Modify: `src/Web/Controllers/HomeController.cs`
- Modify: `src/Web/Views/Home/Index.cshtml`
- Replace: `src/Web/Views/Home/_Banners.cshtml`
- Create: `src/Web/Views/Home/_BannerCarousel.cshtml`
- Modify: `src/Web/wwwroot/css/site.css`
- Test: `tests/Beneficiario.FormTests/Program.cs`

**Interfaces:**
- Consumes: `BannerVm.Posicao` from Task 1.
- Produces: five `List<BannerVm>` properties on `PortalVm`: `BannersLateralEsquerda`, `BannersCentral`, `BannersLateralDireita`, `BannersInferiorEsquerda`, `BannersInferiorDireita`.
- Produces: partial model containing `IReadOnlyList<BannerVm> Banners`, `string CarouselId`, `string AreaLabel`, and `string CssClass`.

- [ ] **Step 1: Write a failing portal-rendering test**

```csharp
var html = await client.GetStringAsync("/");
if (!html.Contains("banner-carousel-central") || html.Contains("banner-carousel-lateral-esquerda"))
    throw new Exception("O portal deve renderizar apenas as posições que possuem banners ativos.");
```

Use a test transport that returns one `Central` active banner and no banners for the other positions.

- [ ] **Step 2: Run the portal test and verify it fails**

Run: `dotnet run --project tests/Beneficiario.FormTests/Beneficiario.FormTests.csproj --no-restore`

Expected: assertion failure because the portal still renders one carrossel genérico `banner-carousel`.

- [ ] **Step 3: Implement grouping, partial reuse and responsive grid**

In `HomeController.Index`, partition `model.Banners` by `Posicao`. Make `Index.cshtml` render the central area plus only populated side and lower areas. The partial emits a Bootstrap carousel with its supplied unique ID, first item active, image URL, label and no controls when a group has one item. Add `.portal-banner-layout` CSS grid: three columns on desktop, two columns at medium widths, one column on mobile. Apply 16:5 only to `.banner-central`; use vertical and horizontal aspect-ratio classes for the other slots. Do not reserve grid cells for empty groups.

- [ ] **Step 4: Run the portal test and verify it passes**

Run: `dotnet run --project tests/Beneficiario.FormTests/Beneficiario.FormTests.csproj --no-restore`

Expected: central-only fixture renders `banner-carousel-central`, omits empty slot IDs, and existing form tests pass.

### Task 4: Carrosséis independentes e regressão do cliente

**Files:**
- Modify: `src/Web/wwwroot/js/portal-carousel.js`
- Test: `tests/Usuario.FormTests/ClientScriptTests.cs`
- Test: `tests/Beneficiario.FormTests/Program.cs`

**Interfaces:**
- Consumes: each partial carousel has class `banner-carousel`, unique `id`, `aria-label`, and one or more `.carousel-item` children.
- Produces: one `bootstrap.Carousel` instance and one controls group per multi-item `.banner-carousel`.

- [ ] **Step 1: Write a failing client test for two independent banner carousels**

```javascript
assert.equal(initializedIds.has('banner-carousel-central'), true);
assert.equal(initializedIds.has('banner-carousel-lateral-esquerda'), true);
assert.equal(document.querySelectorAll('.portal-carousel-controls').length, 2);
```

Use two fixture carousels; verify the previous selector `#banner-carousel` cannot satisfy this test.

- [ ] **Step 2: Run the client script test and verify it fails**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Expected: only the former single banner selector is initialized.

- [ ] **Step 3: Initialize every banner carrossel by class**

Replace `#banner-carousel` in the selector with `.banner-carousel`, retain `#rede-carousel`, and preserve unique `aria-controls` based on `element.id`. Skip control generation for a one-item carousel; retain reduced-motion, hover, focus and touch behavior for every initialized group.

- [ ] **Step 4: Run all focused tests and inspect the responsive portal**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Run: `dotnet run --project tests/Beneficiario.FormTests/Beneficiario.FormTests.csproj --no-restore`

Expected: all tests pass; at desktop the five areas follow the requested composition and on mobile areas stack without an empty gap.

### Task 5: Final integration verification

**Files:**
- Verify: all files above.

- [ ] **Step 1: Restore and build the solution**

Run: `dotnet restore PVHSAUDE.slnx` then `dotnet build PVHSAUDE.slnx --no-restore`

Expected: successful build with zero errors.

- [ ] **Step 2: Execute all project form test executables**

Run: `dotnet run --project tests/Usuario.FormTests/Usuario.FormTests.csproj --no-restore`

Run: `dotnet run --project tests/Beneficiario.FormTests/Beneficiario.FormTests.csproj --no-restore`

Run: `dotnet run --project tests/EmpresaBeneficiada.FormTests/EmpresaBeneficiada.FormTests.csproj --no-restore`

Expected: every executable reports its PASS assertions and exits zero.

- [ ] **Step 3: Check the final diff**

Run: `git diff --check` and `git status --short`

Expected: no whitespace errors; only files listed by this plan plus the approved prior worktree changes are modified.
