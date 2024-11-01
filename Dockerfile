# Fase de build com SDK 8.0
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar apenas os arquivos de projeto primeiro para otimizar cache de camadas
COPY *.csproj ./
RUN dotnet restore

# Copiar todo o resto e build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Fase de runtime com ASP.NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Criar diretório de dados com permissões corretas
RUN mkdir -p /data && chmod 777 /data

# Copiar publicação do build anterior
COPY --from=build /app/publish .

# Expor porta 80 e definir variáveis de ambiente
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "desafio.dll"]