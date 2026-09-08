using PVHSAUDE.Application.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PVHSAUDE.Application.Interface
{
    public interface IAppJwtService
    {
        string GenereteToken(UsuarioVm usuarioVm);
        string GenereteTokenForgotPassword(UsuarioVm usuarioVm);
        string? GetIdentifierToken(string token);
        bool ValidateTokenForgotPassword(string token);

    }
}
