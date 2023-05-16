# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /app

COPY ./src/Api/*.csproj ./Api/
COPY ./src/Application/*.csproj ./Application/
COPY ./src/Domain/*.csproj ./Domain/
COPY ./src/Infrastructure/*.csproj ./Infrastructure/
RUN dotnet restore ./Api/Api.csproj

COPY ./src ./

RUN dotnet publish ./Api/Api.csproj -c Release -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]