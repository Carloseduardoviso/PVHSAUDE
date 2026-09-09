# Usuários da administração

O menu **Usuários** permite listar e cadastrar usuários com nome completo, e-mail, senha e permissão:
Comum = 0, Gestor = 1, Administrador = 2.

Todos os usuários cadastrados podem entrar na administração. Somente Administradores podem listar e cadastrar usuários, tanto no site quanto na API. Não foram definidas outras diferenças entre Comum e Gestor.

As senhas têm entre 8 e 128 caracteres e são armazenadas somente como hash com salt pelo PasswordHasher do ASP.NET Core. O e-mail é único, independentemente de maiúsculas/minúsculas. O login dura até seis horas e tem limite de tentativas por IP.

## Preparação do banco e primeiro acesso

Na raiz do repositório, aplique a migração ao banco configurado para o ambiente desejado:

```powershell
dotnet ef database update --project src/Infra/Infra.Data --startup-project src/Api -- --environment Development
```

Com o banco atualizado, execute localmente:

```powershell
dotnet run --project src/Api --launch-profile https -- --criar-administrador
```

Informe nome completo, e-mail e senha no terminal. A senha não aparece na tela nem é passada na linha de comando. O comando encerra após criar o usuário e recusa execução quando já existe um Administrador. Não há senha padrão nem cadastro inicial público.

Inicie a API e o Web com o perfil https. Acesse /Conta/Login, entre com o administrador e use **Usuários → Cadastrar usuário**.

A migração e o script cadastro-usuarios.sql criam a tabela Usuario e o índice único de e-mail. O script parte da migração EmpresaComPlano; use o comando EF para bancos em versões anteriores.

## Validação

```powershell
dotnet build PVHSAUDE.slnx
dotnet run --project tests/Usuario.FormTests
dotnet run --project tests/Beneficiario.FormTests
```

Os testes de formulário usam transporte HTTP simulado; não gravam no banco.

## Edição, exclusão e status

Na lista de usuários, use Editar, Inativar/Reativar ou Excluir. A exclusão é permanente e exige confirmação na tela. Na edição, deixe a nova senha em branco para manter a atual.

Usuários inativos ou excluídos não podem entrar. A API verifica o estado atual do usuário e a Role em cada chamada autenticada; o site verifica a sessão a cada requisição e encerra o acesso quando ela não é mais válida. Alterar a Role exige novo login.

O último Administrador ativo não pode ser excluído, inativado ou ter a permissão reduzida. A migração UsuarioAtivo mantém os usuários existentes ativos.

Os testes de integração usam exclusivamente um banco LocalDB temporário, removido ao terminar:

```powershell
dotnet run --project tests/Usuario.FormTests -c Release -- --database
```
