# Multi-stage build for .NET API with Nuxt SPA
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Install system dependencies
RUN apt-get update && \
    apt-get install -y unzip curl && \
    rm -rf /var/lib/apt/lists/*

# Install Bun
RUN curl -fsSL https://bun.sh/install | BUN_INSTALL=/usr/local bash -s "bun-v1.2.7"
ENV PATH="/usr/local/bin:$PATH"

# Copy and restore .NET dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy all source files
COPY . ./

# Build the Nuxt SPA first
# WORKDIR /app/ClientApp
# RUN bun install --ignore-scripts
# RUN bun run nuxt prepare
# RUN bun run generate

# Create dist symlink if it doesn't exist (Nuxt generate creates .output/public)
# RUN if [ ! -d "dist" ] && [ -d ".output/public" ]; then ln -sf .output/public dist; fi

# Go back to app root and publish .NET application
WORKDIR /app
RUN dotnet publish -c Release -o /publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install dependencies for PDF generation
RUN apt-get update && \
    apt-get install -y libgdiplus && \
    rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /publish .

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "RadiusConnect.Api.dll"]