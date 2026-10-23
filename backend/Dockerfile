# Stage 1: Build & Publish (Using .NET 9 SDK which natively supports .slnx)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy all project files and restore dependencies
COPY EmployeeLeaveManagementSystem.API/ ./EmployeeLeaveManagementSystem.API/
WORKDIR /src/EmployeeLeaveManagementSystem.API
RUN dotnet restore employee-leave-management-system-api.slnx

# Build and publish release binaries
RUN dotnet publish employee-leave-management-system-api.slnx -c Release -o /app/publish --no-restore

# Stage 2: Runtime Image (Pure .NET 8 runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EmployeeLeaveManagementSystem.API.dll"]
