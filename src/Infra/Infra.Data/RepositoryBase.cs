using Infra.Data.Base;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Domain.Interfaces.Repository;
using PVHSAUDE.Infra.Auth.Interface;
using System.Linq.Expressions;

namespace PVHSAUDE.Infra.Data
{
    public class RepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : class
    {
        protected readonly Context Db;
        protected readonly DbSet<TEntity> DbSet;
        private readonly IAccount? _account;

        public RepositoryBase(Context context, IAccount? account = null)
        {
            Db = context;
            DbSet = Db.Set<TEntity>();
            _account = account;
        }

        public async Task AddAsync(TEntity entity)
        {
            await DbSet.AddAsync(entity);
            await Db.SaveAuditoriaAsync();
        }

        public async Task CommitAsync()
        {
            await Db.SaveChangesAsync();
        }

        public async Task RemoveAsync(TEntity entity)
        {
            DbSet.Remove(entity);
            await Db.SaveAuditoriaAsync();
            await Task.CompletedTask;
        }

        public async Task CheduledCommitAsync(Guid sistemaIdSetContextInfo)
        {
            await Db.SaveAuditoriaAsync();
        }

        public async Task UpdateAsync(TEntity entity)
        {
            DbSet.Attach(entity);
            Db.Entry(entity).State = EntityState.Modified;

            await Db.SaveAuditoriaAsync();
            await Task.CompletedTask;
        }

        protected static string PropertyPath<TSource, TProperty>(Expression<Func<TSource, TProperty>> expression)
        {
            return GetFullPropertyPath(expression.Body);
        }

        private static string GetFullPropertyPath(Expression? expression)
        {
            if (expression is MemberExpression memberExpression)
            {
                // Permite a inclusão (includes) de propriedades aninhadas que não são listas,
                // Exemplo: Participante.EventoDetalhe.Evento
                //Qualquer dúvida, falar com Wesley
                var parentPath = GetFullPropertyPath(memberExpression.Expression);
                return string.IsNullOrEmpty(parentPath) ? memberExpression.Member.Name : $"{parentPath}.{memberExpression.Member.Name}";
            }

            if (expression is UnaryExpression unaryExpression)
            {
                return GetFullPropertyPath(unaryExpression.Operand);
            }

            if (expression is MethodCallExpression methodCallExpression)
            {
                return ExtractSelectMemberPath(methodCallExpression);
            }

            return string.Empty;
        }

        public static string ExtractSelectMemberPath(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2)
            {
                var lambdaExpression = ExtractLambdaExpression(methodCallExpression.Arguments[1]);
                if (lambdaExpression != null)
                {
                    var selectPart = GetFullPropertyPath(lambdaExpression.Body);
                    var pathBeforeSelect = GetFullPropertyPath(methodCallExpression.Arguments[0]);

                    //includes de tabelas meios
                    return $"{pathBeforeSelect}.{selectPart}";
                }
            }

            return GetFullPropertyPath(methodCallExpression.Arguments[0]);
        }
        private static LambdaExpression? ExtractLambdaExpression(Expression expression)
        {
            if (expression is LambdaExpression lambdaExpression)
            {
                return lambdaExpression;
            }

            if (expression is UnaryExpression unaryExpression && unaryExpression.Operand is LambdaExpression innerLambdaExpression)
            {
                return innerLambdaExpression;
            }

            return null;

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}