# =========================
# Etapa 1: Build e publicação
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copia a solução e os projetos
COPY . .

# Restaura as dependências da API e dos projetos referenciados
RUN dotnet restore "src/AlertaCidadao.API/AlertaCidadao.API.csproj"

# Compila e publica a aplicação
RUN dotnet publish "src/AlertaCidadao.API/AlertaCidadao.API.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish


# =========================
# Etapa 2: Imagem de execução
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Copia somente os arquivos publicados
COPY --from=build /app/publish .

# Configura a porta da aplicação no container
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Inicializa a API
ENTRYPOINT ["dotnet", "AlertaCidadao.API.dll"]
