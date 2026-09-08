using AutoMapper;

namespace PVHSAUDE.Application.AppService
{
    public abstract class AppServiceBase<TService> where TService : class
    {
        protected readonly TService Service;
        protected readonly IMapper Mapper;

        protected AppServiceBase(TService service, IMapper mapper)
        {
            Service = service;
            Mapper = mapper;
        }
    }
}