# Stage 1: Build & Publish with .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy all project files across the solution
COPY . .

WORKDIR /src/EmployeeLeaveManagementSystem.API
RUN dotnet restore employee-leave-management-system-api.slnx

# Build and publish release binaries
RUN dotnet publish employee-leave-management-system-api.slnx -c Release -o /app/publish --no-restore

# Stage 2: Runtime Image with .NET 10 ASP.NET Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmployeeLeaveManagementSystem.API.dll"]
