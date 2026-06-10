# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["Area42-1.Web/Area42-1.Web.csproj", "Area42-1.Web/"]
COPY ["Area42-1.ServiceDefaults/Area42-1.ServiceDefaults.csproj", "Area42-1.ServiceDefaults/"]

# Restore packages
RUN dotnet restore "Area42-1.Web/Area42-1.Web.csproj"

# Copy source code
COPY . .

# Build
RUN dotnet build "Area42-1.Web/Area42-1.Web.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Area42-1.Web/Area42-1.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Area42-1.Web.dll"]
