# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY garrard-azure.sln ./
COPY LICENSE ./
COPY src/Garrard.AzureLib/Garrard.AzureLib.csproj src/Garrard.AzureLib/
COPY src/Garrard.Azure.McpServer/Garrard.Azure.McpServer.csproj src/Garrard.Azure.McpServer/
RUN dotnet restore src/Garrard.Azure.McpServer/Garrard.Azure.McpServer.csproj

COPY src/Garrard.AzureLib/ src/Garrard.AzureLib/
COPY src/Garrard.Azure.McpServer/ src/Garrard.Azure.McpServer/
RUN dotnet publish src/Garrard.Azure.McpServer/Garrard.Azure.McpServer.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Default to stdio transport; override with MCP_TRANSPORT=http for HTTP streaming
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Garrard.Azure.McpServer.dll"]
