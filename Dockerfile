# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia arquivos de projeto para restaurar dependências
COPY ["Catalogo.API.sln", "."]
COPY ["Catalogo.API/Catalogo.API.csproj", "Catalogo.API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
RUN dotnet restore "Catalogo.API.sln"

# Publicação do binário
COPY . .
WORKDIR "/src/Catalogo.API"
RUN dotnet publish "Catalogo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime (Otimizado)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS final
WORKDIR /app
EXPOSE 8080

# Instalação limpa do Agente New Relic
RUN apt-get update && apt-get install -y --no-install-recommends wget ca-certificates gnupg \
    && echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
    && wget -qO - https://download.newrelic.com/548C16BF.gpg | apt-key add - \
    && apt-get update \
    && apt-get install -y 'newrelic-dotnet-agent' \
    && rm -rf /var/lib/apt/lists/*

# Variáveis de ambiente New Relic
ENV CORECLR_ENABLE_PROFILING=1 \
    CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A} \
    CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent \
    CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalogo.API.dll"]