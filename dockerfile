# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy everything
COPY . .

# Restore dependencies
RUN dotnet restore QuantityMeasurementAPI/QuantityMeasurementAPI.csproj

# Publish app
RUN dotnet publish QuantityMeasurementAPI/QuantityMeasurementAPI.csproj -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

# Railway port binding
ENV ASPNETCORE_URLS=http://+:${PORT}

ENTRYPOINT ["dotnet", "QuantityMeasurementAPI.dll"]