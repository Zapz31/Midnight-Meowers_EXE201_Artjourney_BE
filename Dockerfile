# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Midnight-Meowers_EXE201_Artjourney.sln .
COPY Artjouney_BE/Artjouney_BE.csproj Artjouney_BE/
COPY BusinessObjects/BusinessObjects.csproj BusinessObjects/
COPY DAOs/DAOs.csproj DAOs/
COPY Helpers/Helpers.csproj Helpers/
COPY Repositories/Repositories.csproj Repositories/
COPY Services/Services.csproj Services/

# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build the solution
WORKDIR /src/Artjouney_BE
RUN dotnet build -c Release --no-restore

# Publish the application
RUN dotnet publish -c Release --no-restore -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Create logs directory
RUN mkdir -p /app/logs && chmod -R 777 /app/logs

# Expose port 8083
EXPOSE 8083

# Configure ASP.NET Core to listen on port 8083
ENV ASPNETCORE_URLS=http://+:8083
ENV ASPNETCORE_ENVIRONMENT=Production

# Add health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
   CMD curl -f http://localhost:8083/api/Test/ping || exit 1

# Run the application
ENTRYPOINT ["dotnet", "Artjouney_BE.dll"]