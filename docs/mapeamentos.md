# Mapeamentos da aplicação

O perfil `src/Application/AutoMapper/AutoMapperConfig.cs` é registrado pela API,
junto ao suporte a expressões. As VMs de aplicação ficam em
`PVHSAUDE.Application.ViewModels`, sem referência ao projeto Web.

Há mapeamentos para as 11 entidades atuais: Plano, Beneficiario, Dependente,
Credenciado, Especialidade, Procedimento, CredenciadoEspecialidade,
CredenciadoProcedimento, Banner, Contato e Usuario.

Na service, use `mapper.Map<PlanoVm>(entidade)` para leitura,
`mapper.Map<Plano>(vm)` para criação e `mapper.Map(vm, entidadeExistente)`
para edição. Os conversores chamam os métodos do domínio para manter a
normalização. IDs enviados pela VM não substituem os IDs da entidade.

Regras para integração das services:

- Validar a VM e os vínculos antes de mapear e persistir.
- Reconciliar dependentes, especialidades e procedimentos explicitamente;
  mapear o pai não substitui suas coleções rastreadas pelo EF.
- Para criar um dependente, fornecer uma entidade construída com o ID do
  beneficiário e os dados validados; a VM de dependente não recebe esse ID.
- Gerar hashes de senha com o password hasher. O perfil nunca mapeia senha
  para hash. Usuários são gravados pelas VMs de cadastro/edição; UsuarioVm
  é uma representação de leitura. LoginVm e LoginResponse não são entidades.
- Processar uploads separadamente. BannerVm contém apenas metadados;
  imagem, content type, datas internas e imagem do credenciado são preservados.
- VMs de composição de tela e upload do Web continuam específicas da interface.

Os controllers da API usam as interfaces das services. As services mapeiam as VMs e
aplicam regras de negócio; os repositories de Infra.Data consultam e persistem entidades.

Validação: `dotnet run --project tests/Usuario.FormTests -c Release --no-launch-profile`.
Inclui validação do perfil, conversão de filtro e preservação de dados na edição.
