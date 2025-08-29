# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AuthSystem.API/AuthSystem.API.csproj", "AuthSystem.API/"]
RUN dotnet restore "AuthSystem.API/AuthSystem.API.csproj"
COPY . .
WORKDIR "/src/AuthSystem.API"
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AuthSystem.API.dll"]
