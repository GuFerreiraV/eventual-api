# Estágio 1: SDK completo só para restaurar e publicar.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Eventual.Api/Eventual.Api.csproj Eventual.Api/
RUN dotnet restore Eventual.Api/Eventual.Api.csproj
COPY Eventual.Api/ Eventual.Api/
RUN dotnet publish Eventual.Api/Eventual.Api.csproj -c Release -o /app --no-restore

# Estágio 2: imagem final só com o runtime, bem menor.
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Eventual.Api.dll"]
