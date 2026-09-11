using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.Logging.Abstractions;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Infra.Data;
using Infra.Data.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PVHSAUDE.Api.Controllers;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Infra.Auth.Interface;
using PVHSAUDE.Infra.Helper.Settings;
using PVHSAUDE.Infra.Ioc;

public static class UsuarioDatabaseTests
{
    public static async Task Run()
    {
        // This database is isolated from the application's database.
        var database = "PVHSAUDE_UsuarioTests_" + Guid.NewGuid().ToString("N");
        var services = new ServiceCollection().AddLogging().AddHttpContextAccessor().AddHttpClient().AddInfrastructure().BuildServiceProvider();
        var options = new DbContextOptionsBuilder<Context>().UseSqlServer(
            $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new Context(options, services.GetRequiredService<IAccount>());
        void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("PASS: " + message); }
        try
        {
            await db.Database.EnsureCreatedAsync();
            var hasher = new PasswordHasher<Usuario>();
            var admin = new Usuario { NomeCompleto = "Admin teste", Email = "admin@example.com", EmailNormalizado = "ADMIN@EXAMPLE.COM", Role = Role.Administrador };
            admin.SenhaHash = hasher.HashPassword(admin, "senha-de-teste");
            db.Add(admin);
            await db.SaveChangesAsync();
            var mapper = new MapperConfiguration(c => { c.AddExpressionMapping(); c.AddProfile<AutoMapperConfig>(); }, NullLoggerFactory.Instance).CreateMapper();
            var work = new UnitOfWork(db);
            var usuarios = new EntityRepository<Usuario>(db);
            var controller = new UsuariosController(new UsuarioService(usuarios, work, mapper, hasher));
            Check(await controller.Inativar(admin.Id, default) is ConflictObjectResult, "Último administrador não pode ser inativado.");
            Check(await controller.Excluir(admin.Id, default) is ConflictObjectResult, "Último administrador não pode ser excluído.");
            Check(await controller.Editar(admin.Id, new UsuarioEdicaoVm { NomeCompleto = admin.NomeCompleto, Email = admin.Email, Role = Role.Comum }, default) is ConflictObjectResult, "Último administrador não pode perder a permissão.");
            Check(await controller.Criar(new UsuarioCadastroVm { NomeCompleto = "Usuário teste", Email = "user@example.com", Senha = "senha-de-teste", Role = Role.Comum, Menus = ["Banner", "Contato"] }, default) is ObjectResult { StatusCode: 201 }, "Usuário criado no SQL Server.");
            var user = await db.Set<Usuario>().SingleAsync(x => x.Email == "user@example.com");
            var oldHash = user.SenhaHash;
            Check(MenusAdministrativos.Ler((await db.Set<Usuario>().AsNoTracking().SingleAsync(x => x.Id == user.Id)).MenusPermitidos).SequenceEqual(new[] { "Banner", "Contato" }), "Menus gravados no banco.");
            var obtido = (UsuarioVm)((OkObjectResult)await controller.Obter(user.Id, default)).Value!;
            Check(obtido.Menus.Length == 2, "Edição recebe as permissões cadastradas.");
            var lista = (IEnumerable<UsuarioVm>)((OkObjectResult)await controller.Listar(default)).Value!;
            Check(lista.Single(x => x.UsuarioId == user.Id).Menus.Length == 2, "Listagem retorna permissões.");
            Check(await controller.Editar(user.Id, new UsuarioEdicaoVm { NomeCompleto = "Alterado", Email = "novo@example.com", Role = Role.Gestor }, default) is NoContentResult && user.SenhaHash == oldHash, "Edição sem senha preserva hash.");
            Check((await db.Set<Usuario>().AsNoTracking().SingleAsync(x => x.Id == user.Id)).MenusPermitidos == "", "Desmarcar todos os menus revoga as permissões no banco.");
            Check(await controller.Editar(user.Id, new UsuarioEdicaoVm { NomeCompleto = "Alterado", Email = "ADMIN@example.com", Role = Role.Gestor }, default) is ConflictObjectResult, "Edição rejeita e-mail duplicado.");
            Check(await controller.Editar(user.Id, new UsuarioEdicaoVm { NomeCompleto = "Alterado", Email = "novo@example.com", Role = Role.Gestor, Senha = "nova-senha-teste" }, default) is NoContentResult && hasher.VerifyHashedPassword(user, user.SenhaHash, "nova-senha-teste") != PasswordVerificationResult.Failed, "Edição altera senha.");
            var jwt = new AppJwtService(Options.Create(new JwtSetting { SecretKey = new string('x', 64), Issuer = "tests", Audience = "tests" }));
            var auth = new AuthController(new AuthService(usuarios, work, mapper, hasher, jwt));
            var login = new LoginVm { Email = user.Email, Senha = "nova-senha-teste" };
            Check(await auth.Login(login, default) is OkObjectResult, "Usuário ativo entra.");
            Check(await controller.Inativar(user.Id, default) is NoContentResult && await auth.Login(login, default) is UnauthorizedResult, "Inativação bloqueia login.");
            Check(await controller.Ativar(user.Id, default) is NoContentResult && await auth.Login(login, default) is OkObjectResult, "Reativação permite login.");
            Check(await controller.Excluir(user.Id, default) is NoContentResult && await auth.Login(login, default) is UnauthorizedResult, "Exclusão remove usuário e bloqueia login.");
            var banners = new BannersController(new BannerService(new BannerRepository(db), work, mapper)) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
            var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAFCAYAAABM6GxJAAABUElEQVR4AQFFAbr+AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFFAAFVYkPxAAAAAElFTkSuQmCC");
            var quadrada = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");
            IFormFile Image(byte[] bytes) => new FormFile(new MemoryStream(bytes), 0, bytes.Length, "Imagem", "banner.png");
            Check(await banners.Criar(new BannerRequest { Titulo = "Quadrado", Imagem = Image(quadrada) }, default) is BadRequestObjectResult, "Banner rejeita imagem fora da proporção paisagem 16:5.");
            Check(await banners.Criar(new BannerRequest { Titulo = "Sem imagem" }, default) is BadRequestObjectResult, "Banner exige imagem.");
            Check(await banners.Criar(new BannerRequest { Titulo = "Inválido", Imagem = Image(new byte[] { 1, 2, 3 }) }, default) is BadRequestObjectResult, "Banner rejeita arquivo com conteúdo inválido.");
            Check(await banners.Criar(new BannerRequest { Titulo = "Destaque", Ativo = true, Imagem = Image(png) }, default) is OkObjectResult, "Banner com imagem criado.");
            var banner = await db.Set<Banner>().SingleAsync();
            Check(await banners.Imagem(banner.Id, default) is FileContentResult { ContentType: "image/png" }, "Imagem ativa disponível publicamente.");
            Check(await banners.Editar(banner.Id, new BannerRequest { Titulo = "Alterado", Ativo = false }, default) is OkObjectResult && banner.Imagem.SequenceEqual(png), "Edição mantém imagem e altera status.");
            var ativos = (OkObjectResult)await banners.Ativos(default);
            Check(System.Text.Json.JsonSerializer.SerializeToElement(ativos.Value).GetArrayLength() == 0, "Portal não lista banner inativo.");
            Check(await banners.Imagem(banner.Id, default) is NotFoundResult, "Imagem inativa não é pública.");
            await banners.Editar(banner.Id, new BannerRequest { Titulo = "Ativo novamente", Ativo = true }, default);
            ativos = (OkObjectResult)await banners.Ativos(default);
            Check(System.Text.Json.JsonSerializer.SerializeToElement(ativos.Value).GetArrayLength() == 1, "Reativação devolve banner ao portal.");
            await ApiDatabaseTests.Run(db, mapper);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
            services.Dispose();
        }
    }
}
