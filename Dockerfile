# Build stage
FROM mcr.microsoft.com/dotnet/sdk:6.0-focal AS build
WORKDIR /app

COPY *.sln . 
COPY ./src/Api/*.csproj ./src/Api/
COPY ./src/Application/*.csproj ./src/Application/
COPY ./src/Domain/*.csproj ./src/Domain/
COPY ./src/Infrastructure/*.csproj ./src/Infrastructure/
RUN dotnet restore ./src/Api/*.csproj

COPY . .
RUN dotnet build

# Unit-test stage
FROM build AS unittest
WORKDIR /app/tests/Application.Tests.Unit/
CMD ["dotnet", "test", "--logger:trx"]

# Integration-test stage
FROM build AS integrationtest
WORKDIR /app/tests/Application.Tests.Integration/
CMD ["dotnet", "test", "--logger:trx"]

# Publish stage
FROM build AS publish
WORKDIR /app/src/Api
RUN dotnet publish -c Release --no-restore -o /app/publish

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:6.0-focal AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]