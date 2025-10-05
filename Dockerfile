# Estágio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo de solução (.sln) e os arquivos de projeto (.csproj)
COPY ["Catalogo.API.sln", "."]
COPY ["Catalogo.API/Catalogo.API.csproj", "Catalogo.API/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Catalogo.APITest/Catalogo.APITest.csproj", "Catalogo.APITest/"]

# Restaura as dependências de todos os projetos
RUN dotnet restore "Catalogo.API.sln"

# Copia todo o resto do código fonte
COPY . .
WORKDIR "/src/Catalogo.API"
RUN dotnet build "Catalogo.API.csproj" -c Release -o /app/build

# Estágio 2: Publicação
FROM build AS publish
RUN dotnet publish "Catalogo.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Estágio 3: Imagem Final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Instala o Agente do New Relic
RUN apt-get update && apt-get install -y wget ca-certificates gnupg \
&& echo 'deb http://apt.newrelic.com/debian/ newrelic non-free' | tee /etc/apt/sources.list.d/newrelic.list \
&& wget https://download.newrelic.com/548C16BF.gpg \
&& apt-key add 548C16BF.gpg \
&& apt-get update \
&& apt-get install -y 'newrelic-dotnet-agent' \
&& rm -rf /var/lib/apt/lists/*

# Configura as variáveis de ambiente para o New Relic
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={36032161-FFC0-4B61-B559-F6C5D41BAE5A}
ENV CORECLR_NEWRELIC_HOME=/usr/local/newrelic-dotnet-agent
ENV CORECLR_PROFILER_PATH=/usr/local/newrelic-dotnet-agent/libNewRelicProfiler.so

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Catalogo.API.dll"]