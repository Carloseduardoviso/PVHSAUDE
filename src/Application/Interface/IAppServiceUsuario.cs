using PVHSAUDE.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PVHSAUDE.Application.Interface
{
    public interface IAppServiceUsuario
    {
        Task AddAsync(UsuarioVm usuarioViewModel);
        Task UpdateAsync(UsuarioVm usuarioViewModel);
        Task UpdateOnlyAsync(UsuarioVm usuarioViewModel);
        Task CheduledUsersAdUpdateAsync(IEnumerable<UsuarioVm> usuariosAdViewModel, Guid sistemaIdSetContextInfo);
        Task<UsuarioVm> GetByIdAsync(Guid usuarioId);
        Task<UsuarioVm?> GetAsync(Expression<Func<UsuarioVm, bool>> expression, params Expression<Func<UsuarioVm, object>>[]? objectsToInclude);
        Task<IEnumerable<UsuarioVm>> GetByNomeUsernameAndSistemaAsync(string nomeUsername, Guid sistemaId);
        Task<IEnumerable<UsuarioVm>> GetAllAsync(Expression<Func<UsuarioVm, bool>> expression, params Expression<Func<UsuarioVm, object>>[] objectsToInclude);
        //Task<(HashSet<UsuarioVm> usuarios, int recordsTotal)> GetPaginationAsync(Expression<Func<Usuario, bool>> filtro, int start, int length);
        Task<(HashSet<UsuarioVm> usuarios, int recordsTotal)> GetPaginationAsync(string search, int start, int length);
        Task<(HashSet<UsuarioVm> usuarios, int recordsTotal)> GetPaginationAsync(string search, int start, int length, Guid sistemaId);
        Task<UsuarioVm> GetByUsernameAsync(string username);
        Task DesvincularSistema(string username, Guid sistemaId);
        Task<IEnumerable<UsuarioVm>> GetEmail(List<Guid> usuarioId);
    }
}
