# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY MyApp.Api.csproj ./
RUN dotnet restore "MyApp.Api.csproj"

COPY . .
RUN dotnet publish "MyApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyApp.Api.dll"]
