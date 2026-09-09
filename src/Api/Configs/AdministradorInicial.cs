using System.ComponentModel.DataAnnotations;
using Infra.Data.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Api.Configs;

public static class AdministradorInicial
{
    public static async Task CriarAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Context>();
        if (await db.Set<Usuario>().AnyAsync(x => x.Role == Role.Administrador))
            throw new InvalidOperationException("Já existe um administrador. Use o cadastro de usuários na área administrativa.");
        Console.Write("Nome completo: ");
        var nome = Console.ReadLine() ?? "";
        Console.Write("E-mail: ");
        var email = Console.ReadLine() ?? "";
        Console.Write("Senha (mínimo de 8 caracteres): ");
        var senha = "";
        ConsoleKeyInfo key;
        while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
        {
            if (key.Key == ConsoleKey.Backspace) { if (senha.Length > 0) senha = senha[..^1]; }
            else if (!char.IsControl(key.KeyChar)) senha += key.KeyChar;
        }
        Console.WriteLine();
        var model = new UsuarioCadastroVm { NomeCompleto = nome, Email = email, Senha = senha, Role = Role.Administrador };
        Validator.ValidateObject(model, new ValidationContext(model), true);
        var usuario = new Usuario { NomeCompleto = nome.Trim(), Email = email.Trim(), EmailNormalizado = email.Trim().ToUpperInvariant(), Role = Role.Administrador };
        usuario.SenhaHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>().HashPassword(usuario, senha);
        db.Add(usuario);
        await db.SaveChangesAsync();
        Console.WriteLine("Administrador criado. Entre no portal com o e-mail e a senha informados.");
    }
}
