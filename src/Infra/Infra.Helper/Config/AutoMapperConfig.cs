using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.Internal;

namespace PVHSAUDE.Infra.Helper.Config
{
    public static class AutoMapperConfig
    {
        public static IMapperConfigurationExpression AddExpressionMapping(this IMapperConfigurationExpression config)
        {
            config.Internal().Mappers.Insert(0, new ExpressionMapper());
            return config;
        }
    }
}