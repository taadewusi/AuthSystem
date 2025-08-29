# ---- Build Stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and restore dependencies
COPY ["AuthSystem.API/AuthSystem.API.csproj", "AuthSystem.API/"]
COPY ["AuthSystem.Application/AuthSystem.Application.csproj", "AuthSystem.Application/"]
COPY ["AuthSystem.Domain/AuthSystem.Domain.csproj", "AuthSystem.Domain/"]
COPY ["AuthSystem.Infrastructure/AuthSystem.Infrastructure.csproj", "AuthSystem.Infrastructure/"]

RUN dotnet restore "AuthSystem.API/AuthSystem.API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/AuthSystem.API"
RUN dotnet publish -c Release -o /app

# ---- Runtime Stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "AuthSystem.API.dll"]
