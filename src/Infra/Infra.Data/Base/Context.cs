using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Infra.Auth.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Infra.Data.Base
{
    public class Context : DbContext
    {
        private IDbContextTransaction? _transaction;
        private readonly IAccount _account;

        public Context(DbContextOptions<Context> options, IAccount account) : base(options)
        {
            _account = account;
        }

        public DbSet<Beneficiario> Beneficiarios { get; set; } = null!;
        public DbSet<Dependente> Dependentes { get; set; } = null!;

        #region Configuração do Modelo

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            RemovePluralizingTableNameConvention(modelBuilder);
            RemoveCascadeDeleteConventions(modelBuilder);

            //modelBuilder.ApplyConfiguration(new UsuarioConfig());

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                modelBuilder.Entity(entityType.ClrType).ToTable(tb => tb.UseSqlOutputClause(false));
            }

            base.OnModelCreating(modelBuilder);
        }

        #endregion

        #region Convenções

        private static void RemovePluralizingTableNameConvention(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entityType.ClrType.Name;
                modelBuilder.Entity(entityType.ClrType).ToTable(tableName);
            }
        }

        private static void RemoveCascadeDeleteConventions(ModelBuilder modelBuilder)
        {
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
        #endregion

        #region AUDITORIA

        public async Task BeginTransactionAsync()
        {
            if (_transaction == null)
            {
                _transaction = await Database.BeginTransactionAsync();
            }
        }

        public async Task SaveAuditoriaAsync()
        {
            try
            {
                await SetContextInfo();
                await SaveChangesAsync();

                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
            catch (DbUpdateException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    Console.WriteLine($"Entidade do tipo \"{entry.Entity.GetType().Name}\" no estado \"{entry.State}\" tem os seguintes erros de validação.");
                }
                await RollbackAsync();
                throw;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"- Mensagem: \"{ex.Message}\", Dados: \"{ex.Data}\"");
                await RollbackAsync();
                throw;
            }
            catch (Exception ex)
            {
                await RollbackAsync();
                throw new InvalidOperationException("Erro ao confirmar a transação.", ex);
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        private async Task SetContextInfo()
        {
            var usuarioId = _account.Current.IsLogado
                ? _account.Current.UsuarioId.ToString()
                : "UsuarioNaoAutenticado";

            var connection = Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = $"DECLARE @UsuarioId VARBINARY(128); SET @UsuarioId = CAST('{usuarioId}' as VARBINARY(128)); SET CONTEXT_INFO @UsuarioId;";
            command.CommandTimeout = 15;

            await command.ExecuteNonQueryAsync();
        }

        #endregion

    }
}
