# Étape 1 : base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Étape 2 : build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier uniquement le fichier csproj pour accélérer le restore
COPY *.csproj ./
RUN dotnet restore

# Copier tout le code et publier
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Étape 3 : final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Entrée principale
ENTRYPOINT ["dotnet", "MicroServicePanier.dll"]
