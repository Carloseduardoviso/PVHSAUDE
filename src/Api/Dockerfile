FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Web/PVHSAUDE.Web.csproj", "src/Web/"]
COPY ["src/Api/PVHSAUDE.Api.csproj", "src/Api/"]
COPY ["src/Application/PVHSAUDE.Application.csproj", "src/Application/"]
COPY ["src/Domain/PVHSAUDE.Domain.csproj", "src/Domain/"]
COPY ["src/Infra/Infra.Auth/PVHSAUDE.Infra.Auth.csproj", "src/Infra/Infra.Auth/"]
COPY ["src/Infra/Infra.Data/PVHSAUDE.Infra.Data.csproj", "src/Infra/Infra.Data/"]
COPY ["src/Infra/Infra.Helper/PVHSAUDE.Infra.Helper.csproj", "src/Infra/Infra.Helper/"]
COPY ["src/Infra/Infra.Ioc/PVHSAUDE.Infra.Ioc.csproj", "src/Infra/Infra.Ioc/"]
RUN dotnet restore "src/Api/PVHSAUDE.Api.csproj"

COPY . .
WORKDIR "/src/src/Api"
RUN dotnet publish "PVHSAUDE.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM build AS migrator
WORKDIR /src
RUN dotnet tool install --tool-path /tools dotnet-ef --version 10.0.11
ENTRYPOINT ["/tools/dotnet-ef"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PVHSAUDE.Api.dll"]
